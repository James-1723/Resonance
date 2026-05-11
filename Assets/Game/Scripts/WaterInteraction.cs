using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    [Header("Player's data")]
    public GameObject player_head, player_hand;
    public Gamemanager gamemanager;
    public AudioController audioController;

    [Header("Water prefabs")]
    public GameObject water_1_1;
    public GameObject water_1_2_rain;
    public GameObject water_3_1;

    [Header("UI")]
    public TextMesh consoleUI;
    public TextMesh consoleUI_1_1;  // 即時顯示頭盔旋轉累積時間
    public TextMesh consoleUI_1_2;  // 即時顯示手部移動累積時間
    public TextMesh consoleUI_1_4;  // 即時顯示頭部位移量

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
