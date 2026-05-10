using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using GDNoob.MyPackage;
using Unity.Netcode;
using TMPro;


public class Gamemanager : NetworkBehaviour
{
    [Header("BGM Setting")]
    public AudioController audioController;

    [Header("Four Interactions")]
    public GameObject[] basicElements;
    public GameObject wind;
    public WindInteraction windInteraction;
    public int roundCount, elementChoice;

    [Header("UI")]
    public TextMesh testConnctionUI;

    [Header("Player's Data")]
    public List<char> charHistorys = new List<char>();
    public List<char> windChars;
    public List<char> fireChars;
    public List<char> waterChars;
    public List<char> dirtChars;

    [Header("Words Exchange")]
    public GameObject textVFX_prefab, player_head;
    public float wordsForwardDist = 1f;
    public float wordsVerticalOffset = -0.5f;
    public float wordsSpacing = 0.5f;

    void Start()
    {
        // Round 1 setting
        roundCount = 0;
        // windChars = new List<char>();
        if (audioController != null)
        {
            audioController.PlayAudio("BGM", 0);
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
            LastRound("Wind");
        }
    }

    void ChooseElement(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i != index)
            {
                basicElements[i].SetActive(false);
            }
        }
    }

    void StopElement()
    {

    }

    void VanishingElement(int index)
    {
        // wait(2);
        // Element should become bigger and start vanishing
        // Need 4 different prompt tones
        basicElements[index].SetActive(false);
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
                    ChooseElement(0);
                    StartCoroutine(WindRound());
                    break;
                case "Fire_Collider":
                    Debug.Log("Fire");
                    ChooseElement(1);
                    break;
                case "Water_Collider":
                    Debug.Log("Water");
                    ChooseElement(2);
                    break;
                case "Dirt_Collider":
                    Debug.Log("Dirt");
                    ChooseElement(3);
                    break;
            }
            roundCount++;
        }
    }

    IEnumerator wait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Debug.Log("Finish Waiting");
    }
    
    IEnumerator WindRound()
    {
        // First Interaction
        yield return wait(3);
        // audioController.PlayAudio("Wind", 0);
        wind.SetActive(true);
        // wind.GetComponent<WindInteraction>().wind_1_1(true);
        // windInteraction.wind_1_1(true);
    }

    void FireRound()
    {
        
    }

    void WaterRound()
    {
        
    }

    void DirtRound()
    {
        
    }

    public void LastRound(string element)
    {
        List<char> chars = element switch
        {
            "Wind"  => windChars,
            "Fire"  => fireChars,
            "Water" => waterChars,
            "Dirt"  => dirtChars,
            _       => null
        };

        if (chars == null || chars.Count == 0) return;

        Vector3 basePos = player_head.transform.position
                        + player_head.transform.forward * wordsForwardDist
                        + Vector3.up * wordsVerticalOffset;
        Vector3 right = player_head.transform.right;
        float totalWidth = (chars.Count - 1) * wordsSpacing;

        for (int i = 0; i < chars.Count; i++)
        {
            GameObject obj = Instantiate(textVFX_prefab);
            Vector3 pos = basePos + right * (i * wordsSpacing - totalWidth / 2f);
            // obj.transform.position = pos;

            Transform vfxChild = obj.transform.Find("Wind_VFX");
            if (vfxChild != null)
                vfxChild.position = pos;

            Transform tmpChild = obj.transform.Find("Wind_TMP");
            if (tmpChild != null)
            {
                TMP_Text tmp = tmpChild.GetComponent<TMP_Text>();
                if (tmp != null)
                    tmp.text = chars[i].ToString();
            }
        }
    }

    public void AddCharToHistory(string element, char c)
    {
        switch (element)
        {
            case "Wind":
                windChars.Add(c);
                break;
            case "Fire":
                fireChars.Add(c);
                break;
            case "Water":
                waterChars.Add(c);
                break;
            case "Dirt":
                dirtChars.Add(c);
                break;
        }
    }

    public void ShowHistory(string element)
    {
        string history = "";
        switch (element)
        {
            case "Wind":
                for (int i = 0; i < windChars.Count; i++)
                {
                    history += windChars[i];
                }
                break;
            case "Fire":
                for (int i = 0; i < fireChars.Count; i++)
                {
                    history += fireChars[i];
                }
                break;
            case "Water":
                for (int i = 0; i < waterChars.Count; i++)
                {
                    history += waterChars[i];
                }
                break;
            case "Dirt":
                for (int i = 0; i < dirtChars.Count; i++)
                {
                    history += dirtChars[i];
                }
                break;
        }
        Debug.Log(history);
    }
}