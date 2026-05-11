using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class WaterInteraction : ParticleControlTemplate
{
    [Header("Water prefabs")]
    public GameObject water_1_1_prefab;
    public GameObject water_1_2_rain_prefab;
    public GameObject water_1_B_hand_water_prefab;
    public GameObject water_3_1_prefab;

    [Header("Water VFX")]
    public VisualEffect water_1_1_vfx;
    public VisualEffect water_3_1_vfx;

    [Header("Water 1 Detection")]
    public float water1_detect_time = 5f;
    public float water1_hand_up_threshold = 0.5f;
    public float water1_head_move_threshold = 0.25f;

    private bool water1PhaseActive = false;
    private bool water1HandUpDetected = false;
    private bool water1HeadMoveDetected = false;
    private float water1StartHandY;
    private Vector3 water1StartHeadPos;

    [Header("Water 2 Detection")]
    public float water2_detect_time = 5f;
    public float water2_head_move_threshold = 0.5f;

    private bool water2PhaseActive = false;
    private bool water2HeadMoveDetected = false;
    private Vector3 water2StartHeadPos;

    [Header("Water 3 Detection")]
    public float water3_detect_time = 5f;
    public float water3_head_move_threshold = 0.5f;
    public float water3_hand_move_threshold = 1f;

    private bool water3PhaseActive = false;
    private bool water3MoveDetected = false;
    private Vector3 water3StartHeadPos;
    private Vector3 water3StartHandPos;

    void Start()
    {
        // audioController.PlayAudio("Water", 0);
        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        // 第一關
        // 初始狀態+下雨
        yield return new WaitForSeconds(1f);
        water_1_1(true);
        yield return new WaitForSeconds(2f);
        water_1_2_rain(true);

        // 判斷手是否舉起來 -> A, B, C
        water1StartHandY = player_hand.transform.position.y;
        water1StartHeadPos = player_head.transform.position;
        water1HandUpDetected = false;
        water1HeadMoveDetected = false;
        water1PhaseActive = true;
        yield return new WaitForSeconds(water1_detect_time);
        water1PhaseActive = false;
        Water_Control_1("start");
        yield return new WaitForSeconds(10f);
        Water_Control_1("end");
        
        // 第二關
        // 凝結初始狀態
        water_2(true);
        yield return new WaitForSeconds(5f);
        water_2_wound(true);

        // 判斷頭部水平位移 -> 2A, 2B
        water2StartHeadPos = player_head.transform.position;
        water2HeadMoveDetected = false;
        water2PhaseActive = true;
        yield return new WaitForSeconds(water2_detect_time);
        water2PhaseActive = false;
        Water_Control_2("start");
        yield return new WaitForSeconds(10f);
        Water_Control_2("end");

        // 第三關
        // 初始狀態
        water_3(true);
        yield return new WaitForSeconds(5f);
        water_3_rise(true);

        // 判斷頭部或手部位移 -> 3A, 3B
        water3StartHeadPos = player_head.transform.position;
        water3StartHandPos = player_hand.transform.position;
        water3MoveDetected = false;
        water3PhaseActive = true;
        yield return new WaitForSeconds(water3_detect_time);
        water3PhaseActive = false;
        Water_Control_3("start");
        yield return new WaitForSeconds(10f);
        Water_Control_3("end");
    }

    void Update()
    {
        if (water1PhaseActive)
        {
            if (player_hand.transform.position.y - water1StartHandY > water1_hand_up_threshold)
                water1HandUpDetected = true;

            Vector3 headDelta = player_head.transform.position - water1StartHeadPos;
            if (new Vector2(headDelta.x, headDelta.z).magnitude > water1_head_move_threshold)
                water1HeadMoveDetected = true;
        }

        if (water2PhaseActive)
        {
            Vector3 headDelta = player_head.transform.position - water2StartHeadPos;
            if (new Vector2(headDelta.x, headDelta.z).magnitude > water2_head_move_threshold)
                water2HeadMoveDetected = true;
        }

        if (water3PhaseActive)
        {
            bool headMoved = Vector3.Distance(player_head.transform.position, water3StartHeadPos) > water3_head_move_threshold;
            bool handMoved = Vector3.Distance(player_hand.transform.position, water3StartHandPos) > water3_hand_move_threshold;
            if (headMoved || handMoved)
                water3MoveDetected = true;
        }
    }

    private void Water_Control_1(string status)
    {
        if (status == "start")
        {
            if (water1HandUpDetected)
            {
                water_1_B(true);
                water_1_B_hand_water(true);
                gamemanager.AddCharToHistory('涵');
            }
            else if (water1HeadMoveDetected)
            {
                water_1_C(true);
                gamemanager.AddCharToHistory('漠');
            }
            else
            {
                water_1_A(true);
                gamemanager.AddCharToHistory('隨');
            }
        }
        else if (status == "end")
        {
            water_1_A(false);
            water_1_B(false);
            water_1_C(false);
        }
    }

    private void Water_Control_2(string status)
    {
        if (status == "start")
        {
            if (water2HeadMoveDetected)
            {
                water_2_A(true);
                gamemanager.AddCharToHistory('流');
            }
            else
            {
                water_2_B(true);
                gamemanager.AddCharToHistory('容');
            }
        }
        else if (status == "end")
        {
            water_2_A(false);
            water_2_B(false);
        }
    }

    private void Water_Control_3(string status)
    {
        if (status == "start")
        {
            if (water3MoveDetected)
            {
                water_3_B(true);
                gamemanager.AddCharToHistory('凝');
            }
            else
            {
                water_3_A(true);
                gamemanager.AddCharToHistory('溫');
            }
        }
        else if (status == "end")
        {
            water_3_A(false);
            water_3_B(false);
        }
    }

    private void water_1_1(bool control) => PrefabControl(water_1_1_prefab, control, 0);
    private void water_1_2_rain(bool control) => PrefabControl(water_1_2_rain_prefab, control, 1);
    private void water_1_A(bool control) => VFXControl(water_1_1_vfx, "Ripple", 2, control);
    private void water_1_B(bool control) => VFXControl(water_1_1_vfx, "B", 3, control);
    private void water_1_B_hand_water(bool control) => PrefabControl(water_1_B_hand_water_prefab, control);
    private void water_1_C(bool control) => VFXControl(water_1_1_vfx, "C", 4, control);
    private void water_2(bool control) => VFXControl(water_1_1_vfx, "Second", 5, control);
    private void water_2_wound(bool control) => VFXControl(water_1_1_vfx, "Wound", 6, control);
    private void water_2_A(bool control) => VFXControl(water_1_1_vfx, "ATurnYellow", 7, control);
    private void water_2_B(bool control) => VFXControl(water_1_1_vfx, "BTurnBrown", 8, control);
    private void water_3(bool control) => PrefabControl(water_3_1_prefab, control, 9);
    private void water_3_rise(bool control) => VFXControl(water_3_1_vfx, "WaterHeight", -1, control);
    private void water_3_A(bool control) => VFXControl(water_3_1_vfx, "ATransparent", -1, control);
    private void water_3_B(bool control) => VFXControl(water_3_1_vfx, "Shoot", -1, control);
}