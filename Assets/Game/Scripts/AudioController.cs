using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource effectPlayer, firePlayer, windPlayer, waterPlayer, dirtPlayer, backupPlayer;
    public AudioClip[] bgmClips, fireClips, windClips, waterClips, dirtClips, backupClips;

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
            case "SoundEffect":
                effectPlayer.clip = bgmClips[index];
                effectPlayer.Play();
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
            case "Addition":
                backupPlayer.clip = backupClips[index];
                backupPlayer.Play();
                break;
        }
    }

    public void StopAudio(string elementType, float fadeDuration = 2f)
    {
        AudioSource target = elementType switch
        {
            "SoundEffect" => effectPlayer,
            "Fire"        => firePlayer,
            "Water"       => waterPlayer,
            "Wind"        => windPlayer,
            "Dirt"        => dirtPlayer,
            "Addition"    => backupPlayer,
            _             => null
        };
        if (target != null)
            StartCoroutine(FadeOut(target, fadeDuration));
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }
}
