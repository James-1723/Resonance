using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

public class UnityAPIServer : MonoBehaviour
{
    [Header("API Settings")]
    public string port = "8080";
    
    private HttpListener listener;
    private Thread listenerThread;
    
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Start()
    {
        StartServer();
    }

    void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    private void StartServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add($"http://*:{port}/");
        listener.Start();
        Debug.Log($"[API Server] 啟動成功，正在監聽 Port: {port}");

        listenerThread = new Thread(ListenForRequests);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    private void ListenForRequests()
    {
        while (listener.IsListening)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[API Server] 監聽中斷或錯誤: {e.Message}");
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/fusion")
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string jsonBody = reader.ReadToEnd();
                Debug.Log($"[API Server] 收到 /api/fusion: {jsonBody}");
                FusionRequestData requestData = JsonUtility.FromJson<FusionRequestData>(jsonBody);
                mainThreadActions.Enqueue(() => {
                    ExecuteFusionLogic(requestData, response);
                });
            }
        }
        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/phase1")
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string jsonBody = reader.ReadToEnd();
                Debug.Log($"[API Server] 收到 /api/phase1: {jsonBody}");
                Phase1RequestData requestData = JsonUtility.FromJson<Phase1RequestData>(jsonBody);
                mainThreadActions.Enqueue(() => {
                    HandlePhase1(requestData, response);
                });
            }
        }
        else
        {
            SendResponse(response, 404, "Not Found");
        }
    }

    private void ExecuteFusionLogic(FusionRequestData data, HttpListenerResponse response)
    {
        Debug.Log($"[API Server] 處理融合 - Host: {data.hostChars}, Client: {data.clientChars}");
        string testData = data.hostChars + data.clientChars;
        // TODO: 換成真正的 LLM 呼叫
        string fusionResult = testData;

        FusionResponseData resData = new FusionResponseData
        {
            success = true,
            fusionResult = fusionResult,
            message = "融合成功（測試模式）"
        };

        SendResponse(response, 200, JsonUtility.ToJson(resData));
    }

    private void HandlePhase1(Phase1RequestData data, HttpListenerResponse response)
    {
        Debug.Log($"[API Server] Phase1 收到 - playerId: {data.playerId}, data: {data.data}");

        Phase1ResponseData resData = new Phase1ResponseData
        {
            success = true,
            message = $"Phase1 資料已接收，playerId={data.playerId}"
        };

        SendResponse(response, 200, JsonUtility.ToJson(resData));
    }

    private void SendResponse(HttpListenerResponse response, int statusCode, string responseString)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;

        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    void OnApplicationQuit()
    {
        if (listener != null)
        {
            listener.Stop();
            listener.Close();
        }
        if (listenerThread != null)
        {
            listenerThread.Abort();
        }
    }
}
