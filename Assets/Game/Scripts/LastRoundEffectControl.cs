using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LastRoundEffectControl : MonoBehaviour
{
    public Gamemanager gamemanager;
    public VisualEffect vfx;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            vfx.SetBool("WordChange", true);
            Destroy(gameObject, 1f);
        }
    }
}
