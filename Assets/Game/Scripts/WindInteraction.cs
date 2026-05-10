using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WindInteraction : MonoBehaviour
{
    [Header("Player's data")]
    public GameObject player_head, player_hand;
    public Gamemanager gamemanager;
    public AudioController audioController;
    
    [Header("Wind 1")]
    public GameObject wind_1_1_scene_atomosphere, wind_1_1_scene_man, wind_1_2_generator;
    public VisualEffect windVFX_1;

    private bool wind1PhaseActive = false;
    private bool wind1ForwardWalkDetected = false;  // 1-4 移動偵測（有做 latch，一旦偵測到就不會取消）
    private float wind1RotationDuration = 0f;       // 1-1 旋轉偵測
    private float wind1HandMoveDuration = 0f;       // 1-2 移動偵測
    private Vector3 wind1PhaseStartHeadPos;
    private Quaternion wind1LastHeadRot;
    private Vector3 wind1LastHandPos;

    public float wind1_detect_time = 3f;
    public float wind1_rotation_threshold = 2f;    // degrees/frame 超過就算「在轉」
    public float wind1_handmove_threshold = 0.01f;  // meters/frame 超過就算「在動」
    public float wind1_walk_threshold = 0.5f;       // head 位移總量超過就算「走了一步」

    [Header("Wind 2")]
    public GameObject wind_2;
    public VisualEffect windVFX_2;
    private Queue<PosRecord> wind2History = new Queue<PosRecord>();
    private float wind2PathLength = 0f;
    private Vector3 wind2LastPos;
    private bool wind2Initialized = false;
    public float wind2_detect_time = 10f;
    private bool wind2PhaseActive = false;
    private bool wind2DetectedLatch = false;

    [Header("Wind 3")]
    public GameObject wind_3;
    public VisualEffect windVFX_3;
    private Vector3 wind3PhaseStartHeadPos;
    private Vector3 wind3PhaseStartForward;
    public float wind3_detect_time = 5f;
    public float wind3_walk_threshold = 0.5f;

    [Header("UI")]
    public TextMesh consoleUI;
    public TextMesh consoleUI_1_1;  // 即時顯示頭盔旋轉累積時間
    public TextMesh consoleUI_1_2;  // 即時顯示手部移動累積時間
    public TextMesh consoleUI_1_4;  // 即時顯示頭部位移量
    
    void Start()
    {
        // wind_1_1_scene_atomosphere.SetActive(false);
        // wind_1_1_scene_man.SetActive(false);
        audioController.PlayAudio("Wind", 0);
        StartCoroutine(GameFlow());
        wind_1_open(true);
    }

    IEnumerator GameFlow()
    {
        // 1 秒緩衝
        yield return new WaitForSeconds(1f);
        consoleUI.text = "結束一秒緩衝，三秒開始";

        // 初始化 phase 變數
        wind1ForwardWalkDetected = false;
        wind1RotationDuration = 0f;
        wind1HandMoveDuration = 0f;
        wind1PhaseStartHeadPos = player_head.transform.position;
        wind1LastHeadRot = player_head.transform.localRotation;
        wind1LastHandPos = player_hand.transform.position;
        wind1PhaseActive = true;

        // 3 秒觀察期
        yield return new WaitForSeconds(wind1_detect_time);
        consoleUI.text = "結束三秒觀察期";
        wind1PhaseActive = false;

        // 根據累積結果決定觸發哪個
        Wind_Control_1("start");
        yield return new WaitForSeconds(10f);
        Wind_Control_1("end");
        consoleUI.text = "第一階段結束十秒互動期";
        gamemanager.ShowHistory("Wind");

        // Second Interaction — accumulate movement data for wind2_detect_time seconds
        audioController.StopAudio("Wind");
        audioController.PlayAudio("Wind", 1);
        wind_2.SetActive(true);
        wind2PhaseActive = true;
        wind2DetectedLatch = false;
        yield return new WaitForSeconds(wind2_detect_time);
        consoleUI.text = "第二階段結束十秒觀察期";

        wind2PhaseActive = false;
        audioController.StopAudio("Wind");
        Wind_Control_2("start");
        yield return new WaitForSeconds(15f);
        consoleUI.text = "第二階段結束十五秒互動期";
        Wind_Control_2("end");
        gamemanager.ShowHistory("Wind");

        // Third Interaction
        audioController.StopAudio("Wind");
        audioController.PlayAudio("Wind", 4);
        wind_3.SetActive(true);
        yield return new WaitForSeconds(3f);
        windVFX_3.SetBool("Inverse", true);
        wind3PhaseStartHeadPos = player_head.transform.position;
        wind3PhaseStartForward = player_head.transform.forward;
        yield return new WaitForSeconds(wind3_detect_time);
        audioController.StopAudio("Wind");
        consoleUI.text = "第三階段結束十秒觀察期";
        Wind_Control_3("start");
        yield return new WaitForSeconds(10f);
        consoleUI.text = "第三階段結束十秒互動期";
        audioController.StopAudio("Wind");
        Wind_Control_3("end");
        gamemanager.ShowHistory("Wind");
    }

    void Update()
    {
        // wind1 data accumulate
        if (wind1PhaseActive)
        {
            AccumulateWind1Data();
        }

        // wind2 data accumulate
        if (wind2PhaseActive)
        {
            if (wind_2_detect())
            {
                wind2DetectedLatch = true;
            }
        }
    }

    private void AccumulateWind1Data()
    {
        // 頭盔旋轉：每幀角度差超過閾值就累加時間
        float angleDiff = Quaternion.Angle(player_head.transform.localRotation, wind1LastHeadRot);
        if (angleDiff >= wind1_rotation_threshold)
        {
            wind1RotationDuration += Time.deltaTime;
        }
        wind1LastHeadRot = player_head.transform.localRotation;
        if (consoleUI_1_1 != null)
        {
            consoleUI_1_1.text = "旋轉時間: " + wind1RotationDuration.ToString("F2") + "s";
        }

        // 手部移動：每幀位移超過閾值就累加時間
        float handDist = Vector3.Distance(player_hand.transform.position, wind1LastHandPos);
        if (handDist >= wind1_handmove_threshold)
        {
            wind1HandMoveDuration += Time.deltaTime;
        }
        wind1LastHandPos = player_hand.transform.position;
        if (consoleUI_1_2 != null)
        {
            consoleUI_1_2.text = "手移時間: " + wind1HandMoveDuration.ToString("F2") + "s";
        }

        // 向前走：從 phase 開始後 head 的總位移量（latch，一旦觸發不重置）
        float totalHeadDist = Vector3.Distance(player_head.transform.position, wind1PhaseStartHeadPos);
        if (totalHeadDist >= wind1_walk_threshold)
        {
            wind1ForwardWalkDetected = true;
        }
        if (consoleUI_1_4 != null)
        {
            consoleUI_1_4.text = "頭部位移: " + totalHeadDist.ToString("F2") + "m" + (wind1ForwardWalkDetected ? " ✓" : "");
        }
    }

    public void wind_1_open(bool control)
    {
        if (control == true)
        {
            consoleUI.text = "Wind_1_open";
            wind_1_1_scene_atomosphere.SetActive(true);
            wind_1_1_scene_man.SetActive(true);        
        }
        else if (control == false)
        {
            consoleUI.text = "Wind_1_close";
            wind_1_1_scene_atomosphere.SetActive(false);
            wind_1_1_scene_man.SetActive(false);        
        }
    }

    public void wind_1_1(bool control)
    {
        if (control == true)
        {
            consoleUI.text = "Wind_1_1_on";
            windVFX_1.SetBool("1 Shinny", true);
        }
        else if (control == false)
        {
            consoleUI.text = "Wind_1_1_off";
            windVFX_1.SetBool("1 Shinny", false);
        }
        
    }

    private void wind_1_2(bool control)
    {
        if (control == true)
        {
            consoleUI.text = "Wind_1_2_on";
            wind_1_2_generator.SetActive(true);
        }
        else if (control == false)
        {
            consoleUI.text = "Wind_1_2_off";
            wind_1_2_generator.SetActive(false);
        }
    }

    public void wind_1_3(bool control)
    {
        if (control == true)
        {
            consoleUI.text = "Wind_1_3_on";
            windVFX_1.SetBool("3 Round cake", true);
        }
        else if (control == false)
        {
            consoleUI.text = "Wind_1_3_off";
            windVFX_1.SetBool("3 Round cake", false);
        }
    }

    private void wind_1_4(bool control)
    {
        if (control == true)
        {
            consoleUI.text = "Wind_1_4_on";
            windVFX_1.SetBool("4 Boom", true);
        }
        else if (control == false)
        {
            consoleUI.text = "Wind_1_4_off";
            windVFX_1.SetBool("4 Boom", false);
        }
    }

    private struct PosRecord
    {
        public float time;
        public Vector3 position;
        public float distanceToPrev;
    }

    private bool wind_2_detect()
    {
        if (!wind2Initialized)
        {
            wind2LastPos = player_head.transform.position;
            wind2Initialized = true;
        }

        Vector3 currentPos = player_head.transform.position;
        float currentTime = Time.time;
        float distToPrev = Vector3.Distance(currentPos, wind2LastPos);

        wind2History.Enqueue(new PosRecord { time = currentTime, position = currentPos, distanceToPrev = distToPrev });
        wind2PathLength += distToPrev;
        wind2LastPos = currentPos;

        // 移除超過5秒的紀錄
        while (wind2History.Count > 0 && currentTime - wind2History.Peek().time > wind2_detect_time)
        {
            PosRecord oldest = wind2History.Dequeue();
            wind2PathLength -= oldest.distanceToPrev;
        }

        // 確保至少有累積接近5秒的數據才進行判斷 (例如至少 4.5 秒)
        if (wind2History.Count > 0 && currentTime - wind2History.Peek().time >= wind2_detect_time * 0.8f)
        {
            Vector3 startPos = wind2History.Peek().position;
            float straightLineDist = Vector3.Distance(startPos, currentPos);

            // 設定位移門檻，避免玩家幾乎沒動 (例如最少需移動 1 單位距離)
            if (wind2PathLength > 0.2f)
            {
                // 計算「直線距離」與「總移動軌跡長度」的比例 (Straightness Index)
                // 若比例接近 1 (例如大於 0.8)，代表這 5 秒內大致上是朝著同一個方向移動
                if (straightLineDist / wind2PathLength > 0.8f)
                {
                    consoleUI.text = "檢測到玩家在 " + wind2_detect_time + " 秒內大致朝同一方向移動！";
                    
                    // 觸發後清空紀錄，重新開始計算，避免連續重複觸發
                    wind2History.Clear();
                    wind2PathLength = 0f;
                    wind2Initialized = false;
                    return true;
                }
            }
        }

        return false;
    }

    private void Wind_Control_1(string status)
    {
        if (status == "start")
        {
            if (wind1ForwardWalkDetected)
            {
                gamemanager.AddCharToHistory("Wind", '動');
                wind_1_4(true);
            }
            else if (wind1RotationDuration >= wind1HandMoveDuration && wind1RotationDuration > 0f)
            {
                gamemanager.AddCharToHistory("Wind", '感');
                wind_1_1(true);
            }
            else if (wind1HandMoveDuration > wind1RotationDuration && wind1HandMoveDuration > 0f)
            {
                gamemanager.AddCharToHistory("Wind", '創');
                wind_1_2(true);
            }
            else
            {
                gamemanager.AddCharToHistory("Wind", '靜');
                wind_1_3(true);
            }
        }
        else if (status == "end")
        {
            wind_1_open(false);
            wind_1_2(false);
        }
    }


    private void Wind_Control_2(string status)
    {
        if (status == "start")
        {
            // 讀取 Update 累積期間的偵測結果，不再重複呼叫 wind_2_detect()
            if (wind2DetectedLatch)
            {
                gamemanager.AddCharToHistory("Wind", '定');
                audioController.PlayAudio("Wind", 2);
                windVFX_2.SetBool("GoAhead", true);
            }
            else
            {
                gamemanager.AddCharToHistory("Wind", '散');
                audioController.PlayAudio("Wind", 3);
                windVFX_2.SetBool("Vortex", true);
            }
        }
        else if (status == "end")
        {
            wind_2.SetActive(false);
        }
    }

    private void Wind_Control_3(string status)
    {
        if (status == "start")
        {
            Vector3 displacement = player_head.transform.position - wind3PhaseStartHeadPos;
            float forwardDist = Vector3.Dot(displacement, wind3PhaseStartForward);
            if (forwardDist >= wind3_walk_threshold)
            {
                gamemanager.AddCharToHistory("Wind", '責');
                audioController.PlayAudio("Wind", 5);
                consoleUI.text = "Wind_3_RoadWidth";
                windVFX_3.SetBool("RoadWidth", true);
            }
            else
            {
                gamemanager.AddCharToHistory("Wind", '逃');
                audioController.PlayAudio("Wind", 6);
                consoleUI.text = "Wind_3_Stop";
                windVFX_3.SetBool("Stop", true);
            }
        }
        else if (status == "end")
        {
            wind_3.SetActive(false);
        }
    }
}