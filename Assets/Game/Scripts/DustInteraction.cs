using System.Collections;
using UnityEngine;

public class DustInteraction : MonoBehaviour
{
    public Gamemanager gamemanager;
    
    [Header("Dust Scene Objects (1-1 ~ 3-2)")]
    public GameObject dust_1_1_Initialize;
    public GameObject dust_1_2_Hole;
    public GameObject dust_1_3_ActionA;
    public GameObject dust_1_4_ActionB;
    public GameObject dust_2_1_Initialize;
    public GameObject dust_2_2_LineFlow;
    public GameObject dust_2_3_Miss;
    public GameObject dust_3_1_Initialize;
    public GameObject dust_3_2_Grow;

    [Header("Timing")]
    [Tooltip("每個效果持續的秒數")]
    public float intervalSeconds = 10f;

    private GameObject[] _sequence;

    void Awake()
    {
        // 建立播放順序陣列（只執行一次）
        _sequence = new GameObject[]
        {
            dust_1_1_Initialize,
            dust_1_2_Hole,
            dust_1_3_ActionA,
            dust_1_4_ActionB,
            dust_2_1_Initialize,
            dust_2_2_LineFlow,
            dust_2_3_Miss,
            dust_3_1_Initialize,
            dust_3_2_Grow,
        };
    }

    void OnEnable()
    {
        // Dust 物件每次打開時：全部關閉，從頭開始播放
        StopAllCoroutines();

        foreach (var obj in _sequence)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        StartCoroutine(GameFlow());
    }

    void OnDisable()
    {
        // Dust 物件關閉時停止播放
        StopAllCoroutines();
    }

    IEnumerator GameFlow()
    {
        for (int i = 0; i < _sequence.Length; i++)
        {
            // 開啟當前效果
            if (_sequence[i] != null)
            {
                _sequence[i].SetActive(true);
                Debug.Log($"DustInteraction [{i + 1}/{_sequence.Length}] 播放中：{_sequence[i].name}");
            }

            // 等待 interval
            yield return new WaitForSeconds(intervalSeconds);

            // 關閉當前效果（準備開下一個）
            if (_sequence[i] != null)
                _sequence[i].SetActive(false);
        }
        gamemanager.AddCharToHistory('培');
        gamemanager.AddCharToHistory('尋');
        gamemanager.AddCharToHistory('峻');
        gamemanager.gameOver = true;
        gamemanager.NotifyPlayerDone();
        Debug.Log("DustInteraction: 所有效果播放完畢");
    }
}
