using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource bgmPlayer, firePlayer, windPlayer, waterPlayer, dirtPlayer;
    public AudioClip[] bgmClips, fireClips, windClips, waterClips, dirtClips;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PlayAudio(string elementType, int index)
    {
        switch (elementType)
        {
            case "BGM":
                bgmPlayer.clip = bgmClips[index];
                bgmPlayer.Play();
                break;
            case "Wind":
                windPlayer.clip = windClips[index];
                windPlayer.Play();
                break;
            case "Fire":
                firePlayer.clip = fireClips[index];
                firePlayer.Play();
                break;
            case "Water":
                waterPlayer.clip = waterClips[index];
                waterPlayer.Play();
                break;
            case "Dirt":
                dirtPlayer.clip = dirtClips[index];
                dirtPlayer.Play();
                break;
        }
    }

    public void StopAudio(string elementType)
    {
        switch (elementType)
        {
            case "BGM":
                bgmPlayer.Stop();
                break;
            case "Fire":
                firePlayer.Stop();
                break;
            case "Water":
                waterPlayer.Stop();
                break;
            case "Wind":
                windPlayer.Stop();
                break;
            case "Dirt":
                dirtPlayer.Stop();
                break;
        }
    }
}
