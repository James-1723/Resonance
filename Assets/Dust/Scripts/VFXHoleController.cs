using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class VFXHoleController : MonoBehaviour
{
    [Header("特效元件 (你的 1-2_Hole 或者總特效物件)")]
    public VisualEffect vfx;

    [Header("7 個土坑的中心點 (放入場景裡的 GameObject)")]
    public Transform[] holeCenters = new Transform[7];

    void Update()
    {
        if (vfx == null) return;
        

        for (int i = 0; i < holeCenters.Length; i++)
        {
            if (holeCenters[i] != null)
            {
                vfx.SetVector3("Pos" + (i + 1), holeCenters[i].position);
            }
        }
    }
}
