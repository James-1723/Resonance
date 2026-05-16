using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class HoleInteractionController : MonoBehaviour
{
    [Header("要控制的特效 (可以是 1-3 或 1-4)")]
    public VisualEffect vfx;

    [Header("這是第幾個坑洞？(1~7)")]
    [Range(1, 7)]
    public int holeIndex = 1;

    [Header("動畫變化的時間 (秒)")]
    public float animationDuration = 2.0f;

    private Coroutine animationCoroutine;

    // 當其他具有 Rigidbody 和 Collider 的物體「碰到」這個觸發器時，會執行這裡
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log("手碰到了第 " + holeIndex + " 號坑！開始填土...");
            // 手碰到時，控制這個坑洞專屬的 FillProgress
            StartAnimation("FillProgress" + holeIndex, 1f);
        }
        else if (other.CompareTag("Foot"))
        {
            Debug.Log("腳踩到了第 " + holeIndex + " 號坑！坑洞擴大...");
            // 腳踩到時，控制這個坑洞專屬的 ExpandProgress
            StartAnimation("ExpandProgress" + holeIndex, 1f); 
        }
    }

    private void StartAnimation(string parameterName, float targetValue)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateProgressRoutine(parameterName, targetValue));
    }

    private IEnumerator AnimateProgressRoutine(string parameterName, float targetValue)
    {
        // 如果 VFX 沒有這個變數，就不執行，避免報錯
        if (vfx == null || !vfx.HasFloat(parameterName))
        {
            Debug.LogWarning("VFX 找不到變數: " + parameterName);
            yield break;
        }

        // 取得目前的數值當作起點
        float startValue = vfx.GetFloat(parameterName);
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsed / animationDuration);
            vfx.SetFloat(parameterName, currentValue);
            yield return null; 
        }

        vfx.SetFloat(parameterName, targetValue);
    }
}
