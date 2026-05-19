using UnityEngine;
using OscJack; // 如果使用 OscJack 套件

public class MadMapperOSCTest : MonoBehaviour
{
    [Header("OSC 設定")]
    public string ipAddress = "127.0.0.1"; // 本機測試
    public int port = 8010;                // MadMapper 預設埠號
    public string oscAddress = "/medias/Video_Card/opacity"; // 替換成你的參數位址
    public string oscAddress2 = "/medias/Video_Card/opacity";

    [Header("測試數值")]
    [Range(0f, 1f)]
    public float sendValue = 1.0f;

    [Header("開關控制")]
    public bool isVisibleA = true; // 在 Inspector 勾選或取消勾選
    public bool isVisibleB = true; // 在 Inspector 勾選或取消勾選

    private OscClient _client;
    private bool _lastVisibleStateA;
    private bool _lastVisibleStateB;

void Start()
    {
        _client = new OscClient(ipAddress, port);
        _lastVisibleStateA = isVisibleA;
        _lastVisibleStateB = isVisibleB;
        // 初始發送一次狀態
        SendToggleA(isVisibleA);
        SendToggleB(isVisibleB);
    }

    void Update()
    {
        // 當勾選狀態改變時才發送
        if (isVisibleA != _lastVisibleStateA)
        {
            SendToggleA(isVisibleA);
            _lastVisibleStateA = isVisibleA;
        }
        if (isVisibleB != _lastVisibleStateB)
        {
            SendToggleB(isVisibleB);
            _lastVisibleStateB = isVisibleB;
        }
    }

    void SendToggleA(bool isOn)
    {
        if (_client != null)
        {
            // 發送 1 (True) 或 0 (False) 給 MadMapper
            _client.Send(oscAddress, isOn ? 1 : 0);
            Debug.Log($"發送 {oscAddress} : {(isOn ? "ON" : "OFF")}");
        }
    }

    void SendToggleB(bool isOn)
    {
        if (_client != null)
        {
            // 發送 1 (True) 或 0 (False) 給 MadMapper
            _client.Send(oscAddress2, isOn ? 1 : 0);
            Debug.Log($"發送 {oscAddress2} : {(isOn ? "ON" : "OFF")}");
        }
    }

    void OnDestroy()
    {
        _client?.Dispose();
    }
}