using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIClient : MonoBehaviour
{
    public string serverIP = "192.168.0.136"; // 172.20.10.2 // 192.168.1.101
    public string serverPort = "8080";

    public void RequestPhase1(string playerId, string data)
    {
        Phase1RequestData reqData = new Phase1RequestData
        {
            playerId = playerId,
            data = data
        };
        string jsonData = JsonUtility.ToJson(reqData);
        StartCoroutine(PostRequest($"http://{serverIP}:{serverPort}/api/phase1", jsonData, null));
    }

    public void RequestFusion(string hostData, string clientData, System.Action<string> onResult = null)
    {
        FusionRequestData reqData = new FusionRequestData
        {
            hostChars = hostData,
            clientChars = clientData
        };
        string jsonData = JsonUtility.ToJson(reqData);
        StartCoroutine(PostRequest($"http://{serverIP}:{serverPort}/api/fusion", jsonData, onResult));
    }

    private IEnumerator PostRequest(string url, string jsonBody, System.Action<string> onSuccess)
    {
        Debug.Log($"[APIClient] 發送請求: {url}");

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"[APIClient] 請求失敗: {request.error}");
        }
        else
        {
            Debug.Log($"[APIClient] 收到回覆: {request.downloadHandler.text}");
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }
}
