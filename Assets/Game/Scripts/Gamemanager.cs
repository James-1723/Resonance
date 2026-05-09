using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using GDNoob.MyPackage;
using Unity.Netcode;
using TMPro;


public class Gamemanager : NetworkBehaviour
{
    public AudioController audioController;

    public GameObject[] basicElements;

    public int roundCount, elementChoice;

    public WindInteraction windInteraction;

    public TextMesh testConnctionUI;

    void Start()
    {
        // Round 1 setting
        roundCount = 0;
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
        wait(2);
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
        audioController.PlayAudio("Wind", 0);
        windInteraction.wind_1_1(true);
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
}