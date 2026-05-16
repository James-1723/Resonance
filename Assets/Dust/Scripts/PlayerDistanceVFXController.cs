using UnityEngine;
using UnityEngine.VFX;

public class PlayerDistanceVFXController : MonoBehaviour
{
    [Header("1. 綁定設定")]
    [Tooltip("要追蹤移動的玩家物件 (如果留空，會自動抓取 Main Camera)")]
    public Transform playerTransform;
    
    [Tooltip("要控制的 3-2 特效")]
    public VisualEffect vfx;

    [Header("2. 距離與進度設定")]
    [Tooltip("要走多遠，特效才能達到『最滿、最亮』的狀態？(公尺)")]
    public float targetDistance = 1.5f; // 預設走 1.5 公尺達到最亮
    
    [Tooltip("如果玩家停下來不走，光芒會慢慢熄滅/變暗嗎？")]
    public bool loseEnergyWhenStopped = true;
    
    [Tooltip("停下來時變暗的速度 (數值越大暗得越快)")]
    public float energyLossRate = 0.5f;

    [Header("3. 觀察面板 (不要手動改它)")]
    [Tooltip("這是目前的進度 0~1，程式會即時算出來並丟給 VFX")]
    [Range(0f, 1f)]
    public float currentProgress = 0f;

    private Vector3 lastPosition;
    private readonly string progressParameter = "MoveProgress";

    void Start()
    {
        // 防呆：如果沒有手動指定玩家，預設抓取主攝影機 (通常 VR 或第一人稱就是追蹤攝影機)
        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (playerTransform != null)
        {
            // 記錄一開始的位置
            lastPosition = playerTransform.position;
        }
    }

    void Update()
    {
        if (playerTransform == null || vfx == null) return;

        // 計算這一個 frame (這零點幾秒內) 玩家移動了多少距離
        float distanceMovedThisFrame = Vector3.Distance(playerTransform.position, lastPosition);
        
        // 更新位置紀錄，準備給下個 frame 比較
        lastPosition = playerTransform.position;

        // 判斷玩家有沒有在移動
        if (distanceMovedThisFrame > 0.001f) // >0.001 是為了過濾掉極微小的抖動
        {
            // 把實際移動的距離轉換成百分比進度，加進總進度裡
            currentProgress += (distanceMovedThisFrame / targetDistance);
        }
        else if (loseEnergyWhenStopped)
        {
            // 玩家沒在動，而且有開啟熄滅設定，就依時間慢慢扣除進度
            currentProgress -= (energyLossRate * Time.deltaTime);
        }

        // 確保進度永遠被限制在 0 (全暗) 到 1 (全亮) 之間
        currentProgress = Mathf.Clamp01(currentProgress);

        // 最重要的一步：把這個 0~1 的數字，丟給 VFX Graph 裡面的 MoveProgress 變數
        if (vfx.HasFloat(progressParameter))
        {
            vfx.SetFloat(progressParameter, currentProgress);
        }
    }
}
