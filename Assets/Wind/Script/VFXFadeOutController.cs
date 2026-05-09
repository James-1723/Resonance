using UnityEngine;
using UnityEngine.VFX;

public class VFXFadeOutController : MonoBehaviour
{
    public VisualEffect vfx;
    public bool StartFade = false;
    public float fadeSpeed = 0.5f;

    private float fadeProgress = 1f;
    private bool lastStartFade = false;

    void Start()
    {
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        if (vfx == null) return;

        vfx.SetBool("StartFade", StartFade);
        vfx.SetFloat("FadeProgress", fadeProgress);
    }

    void Update()
    {
        if (vfx == null)
            vfx = GetComponent<VisualEffect>();

        if (vfx == null) return;

        vfx.SetBool("StartFade", StartFade);

        if (!lastStartFade && StartFade)
        {
            fadeProgress = 1f;
        }

        if (StartFade)
        {
            fadeProgress -= Time.deltaTime * fadeSpeed;
            fadeProgress = Mathf.Clamp(fadeProgress, 0f, 1f);
        }

        vfx.SetFloat("FadeProgress", fadeProgress);

        lastStartFade = StartFade;
    }
}