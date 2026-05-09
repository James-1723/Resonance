using UnityEngine;
using UnityEngine.VFX; // 記得引入 VFX 命名空間

public class VFXDirectionBinder : MonoBehaviour
{
    [Header("目標物體")]
    public Transform targetObject; // 你的測試方塊或玩家

    [Header("VFX 設定")]
    public VisualEffect vfxGraph;  // 你的 VFX 物件
    public string propertyName = "PlayerFace"; // VFX Graph 裡的變數名稱

    // 為了效能，預先轉換字串為 ID
    private int propertyID;

    void Start()
    {
        if (vfxGraph == null) vfxGraph = GetComponent<VisualEffect>();
        propertyID = Shader.PropertyToID(propertyName);
    }

    void Update()
    {
        if (targetObject != null && vfxGraph != null)
        {
            // 提取目標物體的前方向量 (Forward Vector)
            Vector3 forwardDir = targetObject.forward;

            // --- 加入這行除錯 ---

            // 將向量傳送到 VFX Graph 的變數中
            vfxGraph.SetVector3(propertyID, forwardDir);
        }
    }
}