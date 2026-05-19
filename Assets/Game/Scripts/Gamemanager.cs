using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.VFX;


public class Gamemanager : NetworkBehaviour
{
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
    public GameObject textVFX_prefab, player_head;
    public float wordsForwardDist = 1f;
    public float wordsVerticalOffset = -0.5f;
    public float wordsSpacing = 0.5f;

    void Start()
    {
        // charHistorys = new List<char>();
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
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // LastRound();
            SendPhase1DataToServer();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(ExecuteAfterChooseElement(0, WindRound()));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ExecuteAfterChooseElement(1, null));
            // StartCoroutine(FireRound());
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ExecuteAfterChooseElement(2, WaterRound()));
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(ExecuteAfterChooseElement(3, null));
            // StartCoroutine(DirtRound());
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
                    StartCoroutine(ExecuteAfterChooseElement(0, WindRound()));
                    break;
                case "Fire_Collider":
                    Debug.Log("Fire");
                    StartCoroutine(ExecuteAfterChooseElement(1, FireRound()));
                    break;
                case "Water_Collider":
                    Debug.Log("Water");
                    StartCoroutine(ExecuteAfterChooseElement(2, WaterRound()));
                    break;
                case "Dirt_Collider":
                    Debug.Log("Dirt");
                    StartCoroutine(ExecuteAfterChooseElement(3, null));
                    break;
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
        yield return wait(3);
        fire.SetActive(true);
    }

    IEnumerator WaterRound()
    {
        yield return wait(1);
        water.SetActive(true);
    }

    void DirtRound()
    {
        
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
    public void SendPhase1DataToServer()
    {
        if (apiClient == null)
        {
            Debug.LogWarning("[Phase1] APIClient 未設定，無法發送。");
            return;
        }

        string myData = new string(charHistorys.ToArray());
        string role = IsHost ? "host" : "client";

        Debug.Log($"[Phase1] 發送自己的資料給 Server，role={role}, data={myData}");
        apiClient.RequestPhase1(role, myData);
    }

    /// <summary>
    /// 第二階段：Client 把資料送給 Host，Host 彙整後送 PC Server，拿到結果再 Broadcast。
    /// </summary>
    public void SendHistoryToHost()
    {
        string myHistoryData = new string(charHistorys.ToArray());

        if (IsClient && !IsHost)
        {
            myHistoryData = "test"; // TODO: 移除測試用資料
            communicateUI.text = "Client 準備發送資料給 Host...";
            Debug.Log("Client 準備發送資料給 Host...");
            SubmitHistoryServerRpc(myHistoryData, NetworkManager.Singleton.LocalClientId);
        }
        else if (IsHost)
        {
            communicateUI.text = "Host 本身的資料已就緒: " + myHistoryData + "，等待 Client 傳送...";
            Debug.Log($"Host 本身的資料已就緒: {myHistoryData}，等待 Client 傳送...");
        }
    }

    /// <summary>
    /// ServerRpc: 這是 Client 請求 Host (Server) 執行的函式。
    /// RequireOwnership = false 確保即使這個 GameManager 不是該 Client 生成的，Client 也能呼叫此方法。
    /// </summary>
    /// <param name="clientData">Client 傳遞過來的字串資料</param>
    /// <param name="clientId">發送此請求的 Client ID，方便 Host 辨識是誰傳來的</param>
    [ServerRpc(RequireOwnership = false)]
    public void SubmitHistoryServerRpc(string clientData, ulong clientId)
    {
        Debug.Log($"Host 成功接收到來自 Client (ID: {clientId}) 的資料: {clientData}");

        string hostData = string.Join("", this.charHistorys.ToArray());

        if (apiClient != null)
        {
            // 送給 PC Server 處理，等回傳後再 Broadcast
            apiClient.RequestFusion(hostData, clientData, (responseJson) =>
            {
                FusionResponseData resData = JsonUtility.FromJson<FusionResponseData>(responseJson);
                if (resData != null && resData.success)
                {
                    Debug.Log($"[Phase2] Server 回傳融合結果: {resData.fusionResult}");
                    BroadcastCombinedResultClientRpc(resData.fusionResult);
                }
                else
                {
                    Debug.LogWarning("[Phase2] Server 回傳失敗或格式有誤，使用本地合併作為備援。");
                    BroadcastCombinedResultClientRpc(hostData + clientData);
                }
            });
        }
        else
        {
            // APIClient 未設定，直接本地合併
            Debug.LogWarning("[Phase2] APIClient 未設定，直接本地合併。");
            BroadcastCombinedResultClientRpc(hostData + clientData);
        }
    }

    [ClientRpc]
    private void BroadcastCombinedResultClientRpc(string combineData)
    {
        combinedResultUI.text = "融合結果: " + combineData;
    }
}