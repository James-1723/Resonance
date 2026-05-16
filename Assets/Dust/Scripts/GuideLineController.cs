using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
[ExecuteAlways]
public class GuideLineController : MonoBehaviour
{
    [Header("引導線設定 (預設長度3/時間3)")]
    public float lineLength = 3f;
    public float animationDuration = 3.0f;
    
    [Header("【編輯】")]
    [Range(0f, 1f)]
    public float testProgress = 1f;

    [Header("線條外觀 (一般狀態)")]
    public float lineWidth = 0.1f;
    public Color normalColor = new Color(0.4f, 0.2f, 0.05f); // 預設暗棕色

    [Header("線條外觀 (發光狀態)")]
    public Color litColor = new Color(1f, 0.8f, 0.2f); // 發光的金黃色
    public float lightUpDuration = 1.5f;

    private LineRenderer lineRenderer;
    private float currentProgress = 0f;
    private Color currentColor;
    private Coroutine lightUpCoroutine;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        currentColor = normalColor;

        if (Application.isPlaying)
        {
            // 遊戲開始時，從 0 開始畫線動畫
            currentProgress = 0f;
            testProgress = 0f;
        }
    }

    void Update()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        // 強制設定寬度和目前的顏色
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;

        if (Application.isPlaying)
        {
            // 遊戲執行中：隨著時間自動往前畫
            if (currentProgress < 1f)
            {
                currentProgress += (Time.deltaTime / animationDuration);
                if (currentProgress > 1f) currentProgress = 1f;
            }
            DrawLine(currentProgress);
        }
        else
        {
            // 編輯模式：強制保持未發光的暗棕色，並讓妳拖拉桿預覽
            currentColor = normalColor; 
            DrawLine(testProgress);
        }
    }

    private void DrawLine(float progress)
    {
        lineRenderer.positionCount = 2;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + (transform.forward * (lineLength * progress));
        
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    /// <summary>
    /// 給互動腳本呼叫的方法：開始發光動畫
    /// </summary>
    public void LightUpLine()
    {
        if (lightUpCoroutine != null) StopCoroutine(lightUpCoroutine);
        lightUpCoroutine = StartCoroutine(LightUpRoutine());
    }

    private IEnumerator LightUpRoutine()
    {
        float elapsed = 0f;
        Color startColor = currentColor;

        while (elapsed < lightUpDuration)
        {
            elapsed += Time.deltaTime;
            // 顏色平滑過渡 (Lerp)
            currentColor = Color.Lerp(startColor, litColor, elapsed / lightUpDuration);
            yield return null;
        }
        currentColor = litColor;
    }
}
