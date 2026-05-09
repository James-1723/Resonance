using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class _2_1Controller : MonoBehaviour
{
    [Tooltip("請將要控制的 Visual Effect 元件拖曳到這裡。如果留空，程式會嘗試自動抓取同一個物件上的 Visual Effect。")]
    public VisualEffect vfx;

    [Tooltip("漸變動畫的總時間 (秒)")]
    public float transitionDuration = 3f;

    private readonly string turnYellowParameterName = "ATurnYellow";
    private readonly string turnYellowBlendParameterName = "ATurnYellowBlend";

    private readonly string turnBrownParameterName = "BTurnBrown";
    private readonly string turnBrownBlendParameterName = "BTurnBrownBlend";

    private readonly string secondParameterName = "Second";
    private readonly string rippleParameterName = "Ripple";
    private readonly string bParameterName = "B";
    private readonly string cParameterName = "C";

    private bool hasTriggeredYellow = false;
    private bool hasTriggeredBrown = false;
    private bool hasTriggeredSecond = false;

    void Start()
    {
        if (vfx == null)
        {
            vfx = GetComponent<VisualEffect>();
        }

        if (vfx == null)
        {
            Debug.LogError("找不到 Visual Effect 元件！請確認有掛載該元件或是有在 Inspector 中賦值。");
        }
    }

    void Update()
    {
        if (vfx != null)
        {
            if (!hasTriggeredYellow && vfx.HasBool(turnYellowParameterName) && vfx.GetBool(turnYellowParameterName))
            {
                hasTriggeredYellow = true;
                StartCoroutine(AnimateBlend(turnYellowBlendParameterName, 0f, 1f));
            }

            if (!hasTriggeredBrown && vfx.HasBool(turnBrownParameterName) && vfx.GetBool(turnBrownParameterName))
            {
                hasTriggeredBrown = true;
                StartCoroutine(AnimateBlend(turnBrownBlendParameterName, 0f, 1f));
            }

            if (!hasTriggeredSecond && vfx.HasBool(secondParameterName) && vfx.GetBool(secondParameterName))
            {
                hasTriggeredSecond = true;
                
                if (vfx.HasBool(rippleParameterName)) vfx.SetBool(rippleParameterName, false);
                if (vfx.HasBool(bParameterName)) vfx.SetBool(bParameterName, false);
                if (vfx.HasBool(cParameterName)) vfx.SetBool(cParameterName, false);
            }
        }
    }

    private IEnumerator AnimateBlend(string parameterName, float startValue, float endValue)
    {
        float timeElapsed = 0f;

        // 設定起點
        vfx.SetFloat(parameterName, startValue);

        while (timeElapsed < transitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float progress = timeElapsed / transitionDuration;
            
            float currentValue = Mathf.Lerp(startValue, endValue, progress);
            vfx.SetFloat(parameterName, currentValue);
            
            yield return null;
        }

        // 確保終點精確
        vfx.SetFloat(parameterName, endValue);
    }
}
