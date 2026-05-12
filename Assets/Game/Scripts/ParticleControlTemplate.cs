using UnityEngine;
using UnityEngine.VFX;

public class ParticleControlTemplate : MonoBehaviour
{
    [Header("Player's data")]
    public GameObject player_head, player_hand;
    public Gamemanager gamemanager;
    public AudioController audioController;

    [Header("UI")]
    public TextMesh consoleUI;
    public TextMesh consoleUI_1_1;  // 即時顯示頭盔旋轉累積時間
    public TextMesh consoleUI_1_2;  // 即時顯示手部移動累積時間
    public TextMesh consoleUI_1_4;  // 即時顯示頭部位移量

    protected void PrefabControl(string element, GameObject prefab, bool control, int audioIndex = -1)
    {
        if (control && audioIndex >= 0)
            audioController.PlayAudio(element, audioIndex);
        prefab.SetActive(control);
    }

    protected void VFXControl(string element, VisualEffect vfx, string vfxKey, int audioIndex, bool control)
    {
        if (control && audioIndex >= 0)
            audioController.PlayAudio(element, audioIndex);
        vfx.SetBool(vfxKey, control);
    }

    protected void VFXControlFloat(string element, VisualEffect vfx, string vfxKey, int audioIndex, bool control, float value)
    {
        if (control && audioIndex >= 0)
            audioController.PlayAudio(element, audioIndex);
        vfx.SetFloat(vfxKey, value);
    }
}
