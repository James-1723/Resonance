using UnityEngine;
using OscJack; // 如果使用 OscJack 套件

public class MadMapperOSCTest : MonoBehaviour
{
    [Header("OSC 設定")]
    public string ipAddress = "127.0.0.1"; // 本機測試
    public int port = 8010;                // MadMapper 預設埠號
    public string oscAddress = "/medias/Video_Card/opacity"; // 替換成你的參數位址

    [Header("測試數值")]
    [Range(0f, 1f)]
    public float sendValue = 1.0f;

    [Header("開關控制")]
    public bool isVisible = true; // 在 Inspector 勾選或取消勾選

    private OscClient _client;
    private bool _lastVisibleState;

void Start()
    {
        _client = new OscClient(ipAddress, port);
        _lastVisibleState = isVisible;
        // 初始發送一次狀態
        SendToggle(isVisible);
    }

    void Update()
    {
        // 當勾選狀態改變時才發送
        if (isVisible != _lastVisibleState)
        {
            SendToggle(isVisible);
            _lastVisibleState = isVisible;
        }
    }

    void SendToggle(bool isOn)
    {
        if (_client != null)
        {
            // 發送 1 (True) 或 0 (False) 給 MadMapper
            _client.Send(oscAddress, isOn ? 1 : 0);
            Debug.Log($"發送 {oscAddress} : {(isOn ? "ON" : "OFF")}");
        }
    }

    void OnDestroy()
    {
        _client?.Dispose();
    }
}