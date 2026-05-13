using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.VFX;


public class Gamemanager : NetworkBehaviour
{
    [Header("BGM Setting")]
    public AudioController audioController;

    [Header("Four Interactions")]
    public GameObject[] basicElements;
    public GameObject wind;
    public GameObject fire;
    public GameObject water;
    public GameObject dirt;

    [Header("Four Interaction Fade out Effect")]
    public TextToPointsController[] fadeOutEffects;

    [Header("Four Interaction Selected Visual Effect")]
    public VisualEffect windVFX;
    public VisualEffect fireVFX;
    public VisualEffect waterVFX;
    public VisualEffect dirtVFX;

    [Header("UI")]
    public TextMesh testConnctionUI;

    [Header("Player's Data")]
    public List<char> charHistorys;

    [Header("Words Exchange")]
    public GameObject textVFX_prefab, player_head;
    public float wordsForwardDist = 1f;
    public float wordsVerticalOffset = -0.5f;
    public float wordsSpacing = 0.5f;

    void Start()
    {
        charHistorys = new List<char>();
        if (audioController != null)
        {
            audioController.PlayAudio("SoundEffect", 0);
        }

        // Set Connection Status UI
        if (NetworkManager.Singleton.IsHost)
        {
            testConnctionUI.text = "Connection status: Host";
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            testConnctionUI.text = "Connection status: Client";
        }
        else if (NetworkManager.Singleton.IsServer)
        {
            testConnctionUI.text = "Connection status: Server";
        }
        else
        {
            testConnctionUI.text = "Connection status: Unknown";
        }        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LastRound();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(ExecuteAfterChooseElement(0, WindRound()));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(ExecuteAfterChooseElement(1, null));
            // StartCoroutine(FireRound());
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ExecuteAfterChooseElement(2, WaterRound()));
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(ExecuteAfterChooseElement(3, null));
            // StartCoroutine(DirtRound());
        }
    }

    IEnumerator ChooseElement(int index)
    {
        switch (index)
        {
            case 0:
                windVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 6);
                break;
            case 1:
                fireVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 7);
                break;
            case 2:
                waterVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 8);
                break;
            case 3:
                dirtVFX.SetBool("Touch", true);
                audioController.PlayAudio("SoundEffect", 9);
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            if (i != index)
            {
                fadeOutEffects[i].FadeOut = true;
            }
        }

        yield return wait(3);
        fadeOutEffects[index].FadeOut = true;
        yield return wait(6);
        
        for (int i = 0; i < 4; i++)
        {
            basicElements[i].SetActive(false);
        }    
    }

    IEnumerator ExecuteAfterChooseElement(int index, IEnumerator nextCoroutine)
    {
        yield return StartCoroutine(ChooseElement(index));
        if (nextCoroutine != null)
        {
            yield return StartCoroutine(nextCoroutine);
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Particles"))
        {
            string elementName = collider.gameObject.name;
            audioController.StopAudio("BGM");
            switch (elementName)
            {
                case "Wind_Collider":
                    Debug.Log("Wind");
                    StartCoroutine(ExecuteAfterChooseElement(0, WindRound()));
                    break;
                case "Fire_Collider":
                    Debug.Log("Fire");
                    StartCoroutine(ExecuteAfterChooseElement(1, FireRound()));
                    break;
                case "Water_Collider":
                    Debug.Log("Water");
                    StartCoroutine(ExecuteAfterChooseElement(2, WaterRound()));
                    break;
                case "Dirt_Collider":
                    Debug.Log("Dirt");
                    StartCoroutine(ExecuteAfterChooseElement(3, null));
                    break;
            }
        }
    }

    IEnumerator wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Finish Waiting");
    }
    
    IEnumerator WindRound()
    {
        yield return wait(3);
        wind.SetActive(true);
    }

    IEnumerator FireRound()
    {
        yield return wait(3);
        fire.SetActive(true);
    }

    IEnumerator WaterRound()
    {
        yield return wait(1);
        water.SetActive(true);
    }

    void DirtRound()
    {
        
    }

    public void LastRound()
    {
        // List<char> chars = element switch
        // {
        //     "Wind"  => windChars,
        //     "Fire"  => fireChars,
        //     "Water" => waterChars,
        //     "Dirt"  => dirtChars,
        //     _       => null
        // };

        if (charHistorys == null || charHistorys.Count == 0) return;

        Vector3 basePos = player_head.transform.position
                        + player_head.transform.forward * wordsForwardDist
                        + Vector3.up * wordsVerticalOffset;
        Vector3 right = player_head.transform.right;
        float totalWidth = (charHistorys.Count - 1) * wordsSpacing;

        for (int i = 0; i < charHistorys.Count; i++)
        {
            GameObject obj = Instantiate(textVFX_prefab);
            Vector3 pos = basePos + right * (i * wordsSpacing - totalWidth / 2f);
            obj.transform.position = pos;

            // Transform vfxChild = obj.transform.Find("Wind_VFX");
            // if (vfxChild != null)
            //     vfxChild.position = pos;

            Transform tmpChild = obj.transform.Find("Wind_TMP");
            if (tmpChild != null)
            {
                TMP_Text tmp = tmpChild.GetComponent<TMP_Text>();
                if (tmp != null)
                    tmp.text = charHistorys[i].ToString();
            }
        }
    }

    public void AddCharToHistory(char c)
    {
        charHistorys.Add(c);
        // switch (element)
        // {
        //     case "Wind":
        //         windChars.Add(c);
        //         break;
        //     case "Fire":
        //         fireChars.Add(c);
        //         break;
        //     case "Water":
        //         waterChars.Add(c);
        //         break;
        //     case "Dirt":
        //         dirtChars.Add(c);
        //         break;
        // }
    }

    public void ShowHistory()
    {
        string history = "";
        for (int i = 0; i < charHistorys.Count; i++)
        {
            history += charHistorys[i];
        }
        // switch (element)
        // {
        //     case "Wind":
        //         for (int i = 0; i < windChars.Count; i++)
        //         {
        //             history += windChars[i];
        //         }
        //         break;
        //     case "Fire":
        //         for (int i = 0; i < fireChars.Count; i++)
        //         {
        //             history += fireChars[i];
        //         }
        //         break;
        //     case "Water":
        //         for (int i = 0; i < waterChars.Count; i++)
        //         {
        //             history += waterChars[i];
        //         }
        //         break;
        //     case "Dirt":
        //         for (int i = 0; i < dirtChars.Count; i++)
        //         {
        //             history += dirtChars[i];
        //         }
        //         break;
        // }
        Debug.Log(history);
    }
}