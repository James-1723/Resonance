using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class LineInteractionController : MonoBehaviour
{
    [Header("1. 綁定要發光的線條腳本")]
    [Tooltip("請把掛有 GuideLineController 的物件拖進來")]
    public GuideLineController guideLine;

    [Header("2. 成功：綁定往前噴射的粒子 (2-1_LineFlow)")]
    [Tooltip("請把 2-1_LineFlow 特效物件拖進來")]
    public VisualEffect successVFX;

    [Header("3. 失敗：綁定往後流動的粒子 (2-3_MissFlow)")]
    [Tooltip("請把 2-3_MissFlow 特效物件拖進來")]
    public VisualEffect missVFX;

    [Header("判定設定")]
    [Tooltip("腳踩上去後，給玩家幾秒鐘的時間把手放上去？(預設 0.5 秒)")]
    public float handTouchGracePeriod = 0.5f;

    private bool isHandTouching = false;
    private bool hasTriggeredSuccess = false;
    private Coroutine missCheckCoroutine;

    void OnTriggerEnter(Collider other)
    {
        // 如果已經成功觸發過了，這條線的任務就結束了，不再做任何反應
        if (hasTriggeredSuccess) return;

        if (other.CompareTag("Hand"))
        {
            // 手碰到了！
            isHandTouching = true;
            TriggerSuccess();
        }
        else if (other.CompareTag("Foot") || other.CompareTag("Player"))
        {
            // 腳 (或玩家本體) 踩到了！開始倒數計時看看手有沒有伸出來
            if (missCheckCoroutine == null)
            {
                missCheckCoroutine = StartCoroutine(CheckMissRoutine());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isHandTouching = false; // 手離開了
        }
    }

    private IEnumerator CheckMissRoutine()
    {
        // 給玩家一點反應時間 (寬限期)
        yield return new WaitForSeconds(handTouchGracePeriod);

        // 如果時間到了，手還是「沒有」碰在線上，而且還沒成功過
        if (!isHandTouching && !hasTriggeredSuccess)
        {
            Debug.Log("Miss！腳踩上去了，但手沒摸到！觸發往後流動的粒子！");
            if (missVFX != null)
            {
                missVFX.Play();
            }
        }

        missCheckCoroutine = null; // 重置計時器，允許下一次踩踏繼續判定
    }

    private void TriggerSuccess()
    {
        Debug.Log("成功！手碰到了引導線！觸發發光與往前粒子流！");
        hasTriggeredSuccess = true; // 鎖死狀態，這條線已經被點亮了

        // 停止所有正在倒數的 Miss 判定
        if (missCheckCoroutine != null)
        {
            StopCoroutine(missCheckCoroutine);
            missCheckCoroutine = null;
        }

        // 1. 叫引導線發光
        if (guideLine != null)
        {
            guideLine.LightUpLine();
        }

        // 2. 播放成功的往前粒子
        if (successVFX != null)
        {
            successVFX.Play();
        }
        
        // 3. 停止可能會干擾的失敗粒子
        if (missVFX != null)
        {
            missVFX.Stop();
        }
    }
}
