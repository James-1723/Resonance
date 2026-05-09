using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class _3_1Controller : MonoBehaviour
{
    [Tooltip("請將要控制的 Visual Effect 元件拖曳到這裡。如果留空，程式會嘗試自動抓取同一個物件上的 Visual Effect。")]
    public VisualEffect vfx;

    [Tooltip("請將要讀取狀態的另一個 Visual Effect 元件拖曳到這裡。")]
    public VisualEffect sourceVfx;

    [Tooltip("要傳送位置給 PlayerPos 的物體 (例如玩家)")]
    public Transform playerTransform;

    [Tooltip("控制漸變的總時間 (秒)")]
    public float transitionDuration = 3f;

    [Tooltip("當 BShoot 為 True 時，CollideNum 漸進的獨立時間 (秒)")]
    public float shootTransitionDuration = 1.5f;

    [Tooltip("當 BShoot 為 True 時，CollideNum 會漸進到的目標數值")]
    public float targetCollideNum = 10f;

    // 定義 VFX 的參數名稱
    private readonly string parameterName = "WaterHeight";
    private readonly string colorTimeParameterName = "ColorTime";
    private readonly string turnYellowParameterName = "ATurnYellow";
    private readonly string turnBrownParameterName = "BTurnBrown";
    private readonly string transparentParameterName = "ATransparent";
    private readonly string bShootParameterName = "BShoot";
    private readonly string collideNumParameterName = "CollideNum";
    private readonly string playerPosParameterName = "PlayerPos";

    // 狀態追蹤
    private bool hasTriggeredTransparent = false;
    private bool hasTriggeredShoot = false;

    void Start()
    {
        // 如果沒有手動指定 vfx，則嘗試抓取同一個 GameObject 上的 Visual Effect 元件
        if (vfx == null)
        {
            vfx = GetComponent<VisualEffect>();
        }

        if (vfx != null)
        {
            // 開始漸變數值：WaterHeight 從 0 到 20，ColorTime 從 0 到 0.5
            StartCoroutine(AnimateVFXParameters(20f, 0.5f));
        }
        else
        {
            Debug.LogError("找不到 Visual Effect 元件！請確認有掛載該元件或是有在 Inspector 中賦值。");
        }
    }

    void Update()
    {
        if (vfx != null)
        {
            // 持續同步 sourceVfx 的狀態到當前 vfx
            if (sourceVfx != null)
            {
                if (sourceVfx.HasBool(turnYellowParameterName))
                {
                    vfx.SetBool(turnYellowParameterName, sourceVfx.GetBool(turnYellowParameterName));
                }

                if (sourceVfx.HasBool(turnBrownParameterName))
                {
                    vfx.SetBool(turnBrownParameterName, sourceVfx.GetBool(turnBrownParameterName));
                }
            }

            // 隨時檢查當前 VFX 的 ATransparent 參數
            if (!hasTriggeredTransparent && vfx.HasBool(transparentParameterName) && vfx.GetBool(transparentParameterName))
            {
                hasTriggeredTransparent = true;
                // 觸發 ColorTime 從 0.5 漸進到 1
                StartCoroutine(AnimateTransparentColorTime(0.5f, 1f));
            }

            // 檢查 BShoot 參數
            if (!hasTriggeredShoot && vfx.HasBool(bShootParameterName) && vfx.GetBool(bShootParameterName))
            {
                hasTriggeredShoot = true;
                // 觸發 CollideNum 漸進到目標數值
                StartCoroutine(AnimateCollideNum(0f, targetCollideNum));
            }

            // 即時更新 PlayerPos
            if (playerTransform != null && vfx.HasVector3(playerPosParameterName))
            {
                vfx.SetVector3(playerPosParameterName, playerTransform.position);
            }
        }
    }

    private IEnumerator AnimateVFXParameters(float targetWaterHeight, float targetColorTime)
    {
        float timeElapsed = 0f;

        // 設定初始數值為 0
        vfx.SetFloat(parameterName, 0f);
        vfx.SetFloat(colorTimeParameterName, 0f);

        while (timeElapsed < transitionDuration)
        {
            // 隨著時間增加
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / transitionDuration;
            
            // 計算目前時間點對應的漸變數值 (Lerp)
            float currentWaterHeight = Mathf.Lerp(0f, targetWaterHeight, progress);
            float currentColorTime = Mathf.Lerp(0f, targetColorTime, progress);
            
            // 將數值傳遞給 VFX Graph
            vfx.SetFloat(parameterName, currentWaterHeight);
            
            // 只有在 ATransparent 還沒被觸發時，才繼續更新 ColorTime 的初始動畫
            if (!hasTriggeredTransparent)
            {
                vfx.SetFloat(colorTimeParameterName, currentColorTime);
            }
            
            yield return null; // 等待下個 frame
        }

        // 確保結束時的數值是精確的目標數值
        vfx.SetFloat(parameterName, targetWaterHeight);
        
        if (!hasTriggeredTransparent)
        {
            vfx.SetFloat(colorTimeParameterName, targetColorTime);
        }
    }

    private IEnumerator AnimateTransparentColorTime(float startValue, float endValue)
    {
        float timeElapsed = 0f;

        // 先設定為起始值 (0.5)
        vfx.SetFloat(colorTimeParameterName, startValue);

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / transitionDuration;
            
            // 讓 ColorTime 漸進到 1
            float currentColorTime = Mathf.Lerp(startValue, endValue, progress);
            vfx.SetFloat(colorTimeParameterName, currentColorTime);
            
            yield return null;
        }

        vfx.SetFloat(colorTimeParameterName, endValue);
    }

    private IEnumerator AnimateCollideNum(float startValue, float endValue)
    {
        float timeElapsed = 0f;

        vfx.SetFloat(collideNumParameterName, startValue);

        while (timeElapsed < shootTransitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / shootTransitionDuration;
            
            float currentValue = Mathf.Lerp(startValue, endValue, progress);
            vfx.SetFloat(collideNumParameterName, currentValue);
            
            yield return null;
        }

        vfx.SetFloat(collideNumParameterName, endValue);
    }
}
