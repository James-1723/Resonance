using UnityEngine;
using UnityEngine.VFX;

public class VFXMoveEmitter : MonoBehaviour
{
    public Transform targetObject; 
    private VisualEffect vfx;
    private Vector3 lastPosition;

    void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        
        if (targetObject == null)
        {
            // 請確保場景中有一個物件叫這個名字，或者直接在 Inspector 拖入
            GameObject foundObj = GameObject.Find("YourEmptyObjectName");
            if (foundObj != null) targetObject = foundObj.transform;
        }
        
        if (targetObject != null)
        {
            lastPosition = targetObject.position;
        }
    }

    void Update()
    {
        if (targetObject == null || vfx == null) return;

        Vector3 currentPosition = targetObject.position;
        Vector3 displacement = currentPosition - lastPosition; // 位移量

        // 1. 計算速度 (Velocity) - 用於觸發 Spawn over Distance
        Vector3 velocity = displacement / Time.deltaTime;

        // 3. 傳送到 VFX Graph
        vfx.SetVector3("TargetPosition", currentPosition);

        lastPosition = currentPosition;
    }
}