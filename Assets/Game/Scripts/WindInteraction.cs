using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WindInteraction : MonoBehaviour
{
    public GameObject player_head, player_hand;
    
    [Header("Wind 1")]
    public GameObject wind_1_1_scene_atomosphere, wind_1_1_scene_man, wind_1_2_generator;
    public VisualEffect windVFX_1;


    private bool[] wind_1_status = new bool[4] {false, false, false, false};
    private bool[] wind_status_checking = new bool[3] {false, false, false};

    // Detect rotaion & position of player
    private Quaternion lastRotation;
    private Vector3 lastPosition;
    private float rotationTimer = 0f;
    private float positionTimer = 0f;

    [Header("Wind 2")]
    public GameObject wind_2;
    public VisualEffect windVFX_2;
    private Queue<PosRecord> wind2History = new Queue<PosRecord>();
    private float wind2PathLength = 0f;
    private Vector3 wind2LastPos;
    private bool wind2Initialized = false;
    public float wind2_detect_time = 3f;

    private bool wind2PhaseActive = false;
    private bool wind2DetectedLatch = false;

    void Start()
    {
        wind_1_1_scene_atomosphere.SetActive(false);
        wind_1_1_scene_man.SetActive(false);

        if (player_head != null)
        {
            lastRotation = player_head.transform.localRotation;
        }

        if (player_hand != null)
        {
            lastPosition = player_hand.transform.position;
        }

        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        // First Interaction
        WindControl_1("start");
        yield return new WaitForSeconds(10f);
        WindControl_1("end");

        // Second Interaction — accumulate movement data for wind2_detect_time seconds
        wind_2.SetActive(true);
        wind2PhaseActive = true;
        wind2DetectedLatch = false;
        yield return new WaitForSeconds(wind2_detect_time);

        wind2PhaseActive = false;
        Wind_Control_2("start");
        yield return new WaitForSeconds(10f);
        Wind_Control_2("end");
    }

    void Update()
    {
        // 在 wind2 收集階段每幀記錄一次位置
        if (wind2PhaseActive)
        {
            if (wind_2_detect())
            {
                wind2DetectedLatch = true;
            }
        }

        // For Testing
        if (Input.GetKeyDown(KeyCode.S))
        {
            wind_1_1(true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            wind_1_3(true);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            wind_1_4(true);
        }
    }

    void WindControl_1(string status)
    {
        if (status == "start")
        {
            if (wind_1_4_detect())
            {
                wind_1_4(true);
            }
            else if (wind_1_2_detect())
            {
                wind_1_2(true);
            }
            else if (wind_1_1_detect())
            {
                wind_1_1(true);
            }
            else if (wind_1_3_detect())
            {
                wind_1_3(true);
            }
        }
        else if (status == "end")
        {
            wind_1_4(false);
            wind_1_2(false);
            wind_1_1(false);
            wind_1_3(false);
        }
    }

    // Spin -> Particle get bright
    private bool wind_1_1_detect()
    {
        rotationTimer += Time.deltaTime;
        
        // 每 1 秒檢查一次旋轉差異
        if (rotationTimer >= 1f)
        {
            float angleDiff = Quaternion.Angle(player_head.transform.localRotation, lastRotation);
            if (angleDiff >= 20f)
            {
                // 當任一方向的旋轉與上一秒相差 20 度時，會執行這裡
                Debug.Log("玩家在 1 秒內旋轉超過 20 度！(相差: " + angleDiff + " 度)");
                wind_1_status[0] = true;
                return true;
                // wind_1_1_on();
            }
            
            // 記錄當下的旋轉值給下一秒比對
            lastRotation = player_head.transform.localRotation;
            rotationTimer = 0f;
        }
        return false;
    }

    public void wind_1_1(bool control)
    {
        if (control == true)
        {
            Debug.Log("Wind_1_1_on");
            wind_1_1_scene_atomosphere.SetActive(true);
            wind_1_1_scene_man.SetActive(true);
        }
        else if (control == false)
        {
            Debug.Log("Wind_1_1_off");
            wind_1_1_scene_atomosphere.SetActive(false);
            wind_1_1_scene_man.SetActive(false);
        }
        
    }

    // Particle generate along with player's hand position
    private bool wind_1_2_detect()
    {
        positionTimer += Time.deltaTime;

        if (positionTimer >= 1f)
        {
            float distanceDiff = Vector3.Distance(player_hand.transform.position, lastPosition);
            if (distanceDiff >= 1f)
            {
                // 當任一方向的距離與上一秒相差 1f 時，會執行這裡
                Debug.Log("玩家在 1 秒內移動超過 1f！(相差: " + distanceDiff + "f)");
                wind_1_status[1] = true;
                return true;
            }
            
            // 記錄當下的位置值給下一秒比對
            lastPosition = player_hand.transform.position;
            positionTimer = 0f;
        }
        return false;
    }

    private void wind_1_2(bool control)
    {
        if (control == true)
        {
            Debug.Log("Wind_1_2_on");
            wind_1_2_generator.SetActive(true);
        }
        else if (control == false)
        {
            Debug.Log("Wind_1_2_off");
            wind_1_2_generator.SetActive(false);
        }
    }

    // Particle gathering
    private bool wind_1_3_detect()
    {
        if (wind_1_status[0] == false && wind_1_status[1] == false)
        {
            return true;
        }
        return false;
    }

    public void wind_1_3(bool control)
    {
        if (control == true)
        {
            Debug.Log("Wind_1_3_on");
            windVFX_1.SetBool("3 Round cake", true);
        }
        else if (control == false)
        {
            Debug.Log("Wind_1_3_off");
            windVFX_1.SetBool("3 Round cake", false);
        }
    }

    // Particle explode as player move
    private bool wind_1_4_detect()
    {
        positionTimer += Time.deltaTime;

        if (positionTimer >= 1f)
        {
            float distanceDiff = Vector3.Distance(player_head.transform.position, lastPosition);
            if (distanceDiff >= 1f)
            {
                // 當任一方向的距離與上一秒相差 1f 時，會執行這裡
                Debug.Log("玩家在 1 秒內移動超過 1f！(相差: " + distanceDiff + "f)");
                wind_1_status[1] = true;
                return true;
            }
            
            // 記錄當下的位置值給下一秒比對
            lastPosition = player_hand.transform.position;
            positionTimer = 0f;
        }
        return false;
    }

    private void wind_1_4(bool control)
    {
        if (control == true)
        {
            Debug.Log("Wind_1_4_on");
            windVFX_1.SetBool("4 Boom", true);
        }
        else if (control == false)
        {
            Debug.Log("Wind_1_4_off");
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
            if (wind2PathLength > 1.0f)
            {
                // 計算「直線距離」與「總移動軌跡長度」的比例 (Straightness Index)
                // 若比例接近 1 (例如大於 0.8)，代表這 5 秒內大致上是朝著同一個方向移動
                if (straightLineDist / wind2PathLength > 0.8f)
                {
                    Debug.Log("檢測到玩家在 " + wind2_detect_time + " 秒內大致朝同一方向移動！");
                    
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

    private void Wind_Control_2(string status)
    {
        if (status == "start")
        {
            // 讀取 Update 累積期間的偵測結果，不再重複呼叫 wind_2_detect()
            if (wind2DetectedLatch)
            {
                windVFX_2.SetBool("GoAhead", true);
            }
            else
            {
                windVFX_2.SetBool("Vortex", true);
            }
        }
        else if (status == "end")
        {
            windVFX_2.SetBool("GoAhead", false);
            windVFX_2.SetBool("Vortex", false);
        }
    }
}