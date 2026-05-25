using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.VFX;


public class Gamemanager : NetworkBehaviour
{
    [Header("Connection")]
    public GameObject isHostCube;
    public GameObject isClientCube;
    public Material checkMaterial;
    
    [Header("BGM Setting")]
    public AudioController audioController;

    [Header("Four Interactions")]
    public GameObject[] basicElements;
    public GameObject wind;
    public GameObject fire;
    public GameObject water;
    public GameObject dirt;

    [Header("Four Interaction Fade out Effect")]
    public TextToPointsController[] fadeOutEffects;

    [Header("Four Interaction Selected Visual Effect")]
    public VisualEffect windVFX;
    public VisualEffect fireVFX;
    public VisualEffect waterVFX;
    public VisualEffect dirtVFX;

    [Header("UI")]
    public TextMesh testConnctionUI;
    public TextMesh communicateUI;
    public TextMesh combinedResultUI;
    public TextMesh ngoLogUI;

    [Header("API")]
    public APIClient apiClient;

    [Header("Player's Data")]
    public List<char> charHistorys;

    [Header("Words Exchange")]
    public GameObject wordGroup;
    public GameObject textVFX_prefab, player_head;
    public float wordsForwardDist = 1f;
    public float wordsVerticalOffset = -0.5f;
    public float wordsSpacing = 0.5f;
    public GameObject[] hostChars;
    public GameObject[] clientChars;

    [Header("GameFlow Setting")]
    public bool gameOver = false;
    public bool[] interactionTriggered = new bool[4]{false, false, false, false};
    public bool isHostDone = false;
    public bool isClientDone = false;
    public bool bothPlayersDone = false;
    public int activeRandomIndex = -1;
    public bool hostSelectWord = false;
    public bool clientSelectWord = false;
    public string hostSelectedWord = "";
    public string clientSelectedWord = "";

    void Start()
    {
        charHistorys = new List<char>();
        if (audioController != null)
        {
            audioController.PlayAudio("SoundEffect", 0);
        }

        // Set Connection Status UI
        if (NetworkManager.Singleton.IsHost)
        {
            testConnctionUI.text = "Connection status: Host";
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            testConnctionUI.text = "Connection status: Client";
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            testConnctionUI.text = "Connection status: Server";
        }
        else
        {
            testConnctionUI.text = "Connection status: Unknown";
        }        
    }
    void Update()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            ngoLogUI.text =
                $"IsHost: {nm.IsHost} | IsClient: {nm.IsClient}\n" +
                $"IsListening: {nm.IsListening} | IsConnectedClient: {nm.IsConnectedClient}\n" +
                $"ClientId: {nm.LocalClientId} | ConnectedClients: {nm.ConnectedClientsIds.Count}";

            if (nm.IsHost)
            {
                Renderer r = isHostCube.GetComponent<Renderer>();
                if (r != null) r.material = checkMaterial;
            }
            if (nm.IsClient)
            {
                Renderer r = isClientCube.GetComponent<Renderer>();
                if (r != null) r.material = checkMaterial;
            }
        }

        gameOver = false;
        if (gameOver == true)
        {
            gameOver = false;

            List<int> remaining = new List<int>();
            for (int i = 0; i < interactionTriggered.Length; i++)
            {
                if (!interactionTriggered[i]) remaining.Add(i);
            }

            if (remaining.Count > 0)
            {
                activeRandomIndex = remaining[Random.Range(0, remaining.Count)];
                ChooseElement(activeRandomIndex);
                switch (activeRandomIndex)
                {
                    case 0: WindRound(); break;
                    case 1: FireRound(); break;
                    case 2: WaterRound(); break;
                    case 3: DirtRound(); break;
                }
            }

        }
    }

    IEnumerator ChooseElement(int index)
    {
        switch (index)
        {
            case 0:
                windVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 6);
                break;
            case 1:
                fireVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 7);
                break;
            case 2:
                waterVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 8);
                break;
            case 3:
                dirtVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 9);
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            if (i != index)
            {
                fadeOutEffects[i].FadeOut = true;
            }
        }

        yield return wait(3);
        fadeOutEffects[index].Off = true;
        yield return wait(6);
        
        for (int i = 0; i < 4; i++)
        {
            basicElements[i].SetActive(false);
        }    
    }

    IEnumerator ExecuteAfterChooseElement(int index, IEnumerator nextCoroutine)
    {
        yield return StartCoroutine(ChooseElement(index));
        if (nextCoroutine != null)
        {
            yield return StartCoroutine(nextCoroutine);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Particles"))
        {
            string elementName = collider.gameObject.name;
            audioController.StopAudio("BGM");
            switch (elementName)
            {
                case "Wind_Collider":
                    Debug.Log("Wind");
                    SendPhase1DataToServer("wind");
                    StartCoroutine(ExecuteAfterChooseElement(0, WindRound()));
                    break;
                case "Fire_Collider":
                    Debug.Log("Fire");
                    SendPhase1DataToServer("fire");
                    StartCoroutine(ExecuteAfterChooseElement(1, FireRound()));
                    break;
                case "Water_Collider":
                    Debug.Log("Water");
                    SendPhase1DataToServer("water");
                    StartCoroutine(ExecuteAfterChooseElement(2, WaterRound()));
                    break;
                case "Dirt_Collider":
                    Debug.Log("Dirt");
                    SendPhase1DataToServer("dirt");
                    StartCoroutine(ExecuteAfterChooseElement(3, DirtRound()));
                    break;
            }
        }

        if (collider.gameObject.CompareTag("Words"))
        {
            if (!bothPlayersDone) return;

            if (charHistorys == null || charHistorys.Count == 0) return;
            string word = charHistorys[0].ToString();

            if (IsHost && !hostSelectWord)
            {
                hostSelectWord = true;
                hostSelectedWord = word;
                CheckBothWordsSelected();
            }
            else if (!IsHost && !clientSelectWord)
            {
                clientSelectWord = true;
                clientSelectedWord = word;
                NotifyHostWordSelectedServerRpc(word);
            }
        }
    }

    IEnumerator wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Finish Waiting");
    }
    
    IEnumerator WindRound()
    {
        yield return wait(3);
        wind.SetActive(true);
    }

    IEnumerator FireRound()
    {
        yield return wait(1);
        fire.SetActive(true);
    }

    IEnumerator WaterRound()
    {
        yield return wait(1);
        water.SetActive(true);
    }

    IEnumerator DirtRound()
    {
        yield return wait(1);
        dirt.SetActive(true);
    }

    public void LastRound()
    {
        if (charHistorys == null || charHistorys.Count == 0) return;

        Vector3 basePos = player_head.transform.position
                        + player_head.transform.forward * wordsForwardDist
                        + Vector3.up * wordsVerticalOffset;
        Vector3 right = player_head.transform.right;
        float totalWidth = (charHistorys.Count - 1) * wordsSpacing;

        for (int i = 0; i < charHistorys.Count; i++)
        {
            GameObject obj = Instantiate(textVFX_prefab);
            Vector3 pos = basePos + right * (i * wordsSpacing - totalWidth / 2f);
            obj.transform.position = pos;

            Transform tmpChild = obj.transform.Find("Wind_TMP");
            if (tmpChild != null)
            {
                TMP_Text tmp = tmpChild.GetComponent<TMP_Text>();
                if (tmp != null)
                    tmp.text = charHistorys[i].ToString();
            }
        }
    }

    public void AddCharToHistory(char c)
    {
        charHistorys.Add(c);
    }

    public void ShowHistory()
    {
        string history = "";
        for (int i = 0; i < charHistorys.Count; i++)
        {
            history += charHistorys[i];
        }
        Debug.Log(history);
    }

    // ==========================================
    // 多人連線資料交換邏輯 (Data Exchange)
    // ==========================================

    /// <summary>
    /// 準備將自己的資料發送給 Host。
    /// 可以綁定在某個 UI 按鈕上，或是融合階段開始時呼叫。
    /// </summary>
    /// <summary>
    /// 第一階段：各自獨立將自己的字串送給 PC Server，不需要等對方。
    /// </summary>
    public void SendPhase1DataToServer(string element)
    {
        if (apiClient == null)
        {
            Debug.LogWarning("[Phase1] APIClient 未設定，無法發送。");
            return;
        }
        string myData = element;
        string role = IsHost ? "host" : "client";

        Debug.Log($"[Phase1] 發送自己的資料給 Server，role={role}, data={myData}");
        apiClient.RequestPhase1(role, myData);
    }

    /// <summary>
    /// 第二階段 (步驟 1)：Client 把自己的資料送給 Host，Host 收到後將雙方的前三個字顯示在畫面上。
    /// </summary>
    public void SyncHistoryDataToHost()
    {
        string myHistoryData = new string(charHistorys.ToArray());
        if (IsClient && !IsHost)
        {
            // myHistoryData = "EFG"; // TODO: 移除測試用資料
            communicateUI.text = "Client 準備發送資料給 Host 作為顯示...";
            Debug.Log("Client 準備發送資料給 Host 作為顯示...");
            SyncHistoryServerRpc(myHistoryData);
        }
        else if (IsHost)
        {
            communicateUI.text = "Host 本身的資料已就緒: " + myHistoryData + "，等待 Client 傳送以顯示...";
            Debug.Log($"Host 本身的資料已就緒: {myHistoryData}，等待 Client 傳送以顯示...");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncHistoryServerRpc(string clientData)
    {
        string hostData = string.Join("", this.charHistorys.ToArray());
        UpdateHistoryDisplayClientRpc(hostData, clientData);
    }

    [ClientRpc]
    public void UpdateHistoryDisplayClientRpc(string hostData, string clientData)
    {
        // 顯示雙方前三個字 (共六個字)，觸發各自的 Touch VFX
        for(int i = 0; i < 3; i++) 
        {
            if (i < hostData.Length && i < hostChars.Length) 
            {
                SetTouchVFX(hostChars[i]);
                SetTextVFX(hostChars[i], hostData[i].ToString());
            }
            if (i < clientData.Length && i < clientChars.Length) 
            {
                SetTouchVFX(clientChars[i]);
                SetTextVFX(clientChars[i], clientData[i].ToString());
            }
        }
    }

    private void SetTouchVFX(GameObject obj)
    {
        if (obj == null) return;
        VisualEffect vfx = obj.GetComponentInChildren<VisualEffect>(true);
        if (vfx != null) vfx.SetBool("Touch", true);
    }

    private void SetTextVFX(GameObject obj, string text)
    {
        if (obj == null) return;
        Transform tmpChild = obj.transform.Find("Wind_TMP");
        if (tmpChild != null)
        {
            TMP_Text tmp = tmpChild.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = text;
        }
    }

    public void NotifyPlayerDone()
    {
        fire.SetActive(false);
        Debug.Log("NotifyPlayerDone");
        if (IsHost)
        {
            isHostDone = true;
            CheckBothPlayersReady();
        }
        else
        {
            NotifyHostDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyHostDoneServerRpc()
    {
        isClientDone = true;
        CheckBothPlayersReady();
    }

    private void CheckBothPlayersReady()
    {
        if (isHostDone && isClientDone)
        {
            TriggerSyncClientRpc();
            wind.SetActive(false);
            // fire.SetActive(false);
            water.SetActive(false);
            // dirt.SetActive(false);
        }
    }

    [ClientRpc]
    private void TriggerSyncClientRpc()
    {
        bothPlayersDone = true;

        switch (activeRandomIndex)
        {
            case 0: if (wind  != null) wind.SetActive(false);  break;
            case 1: if (fire  != null) fire.SetActive(false);  break;
            case 2: if (water != null) water.SetActive(false); break;
            case 3: if (dirt  != null) dirt.SetActive(false);  break;
        }
        activeRandomIndex = -1;

        if (!IsHost) SyncHistoryDataToHost();
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyHostWordSelectedServerRpc(string word)
    {
        clientSelectWord = true;
        clientSelectedWord = word;
        CheckBothWordsSelected();
    }

    private void CheckBothWordsSelected()
    {
        if (hostSelectWord && clientSelectWord)
            TriggerFusionClientRpc();
    }

    [ClientRpc]
    private void TriggerFusionClientRpc()
    {
        if (!IsHost) RequestFusionDataToHost();
    }

    /// <summary>
    /// 第二階段 (步驟 2)：Client 將自己的資料送給 Host，再由 Host 送去 Server，取得回傳的融合字後顯示在第四個位置。
    /// </summary>
    public void RequestFusionDataToHost()
    {
        string myHistoryData = new string(charHistorys.ToArray());
        if (IsClient && !IsHost)
        {
            communicateUI.text = "Client 準備發送資料給 Host 以進行融合...";
            Debug.Log("Client 準備發送資料給 Host 以進行融合...");
            RequestFusionServerRpc(clientSelectedWord, NetworkManager.Singleton.LocalClientId);
        }
        else if (IsHost)
        {
            communicateUI.text = "Host 準備與 Client 資料融合: " + myHistoryData + "，等待 Client 傳送...";
            Debug.Log($"Host 準備與 Client 資料融合: {myHistoryData}，等待 Client 傳送...");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestFusionServerRpc(string clientData, ulong clientId)
    {
        Debug.Log($"Host 成功接收到來自 Client (ID: {clientId}) 的資料: {clientData}，準備請求融合");

        string hostData = hostSelectedWord;

        if (apiClient != null)
        {
            apiClient.RequestFusion(hostData, clientData, (responseJson) =>
            {
                FusionResponseData resData = JsonUtility.FromJson<FusionResponseData>(responseJson);
                if (resData != null && resData.success)
                {
                    Debug.Log($"[Phase2] Server 回傳融合結果: {resData.fusionResult}");
                    ApplyFusionResultClientRpc(resData.fusionResult);
                }
                else
                {
                    Debug.LogWarning("[Phase2] Server 回傳失敗或格式有誤，使用備援字。");
                    ApplyFusionResultClientRpc(""); // fallback
                }
            });
        }
        else
        {
            Debug.LogWarning("[Phase2] APIClient 未設定，直接使用備援字。");
            ApplyFusionResultClientRpc("");
        }
    }

    [ClientRpc]
    public void ApplyFusionResultClientRpc(string fusionResult)
    {
        string res = string.IsNullOrEmpty(fusionResult) ? "" : fusionResult;
        char fChar = res[0]; // 拿回傳的那一個字

        if (hostChars.Length > 3)
        {
            SetTouchVFX(hostChars[3]);
            SetTextVFX(hostChars[3], fChar.ToString());
        }
        if (clientChars.Length > 3)
        {
            SetTouchVFX(clientChars[3]);
            SetTextVFX(clientChars[3], fChar.ToString());
        }

        if (combinedResultUI != null)
        {
            combinedResultUI.text = "融合結果: " + res;
        }
    }
}