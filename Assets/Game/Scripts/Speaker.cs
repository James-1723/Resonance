using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public string text;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("開機！");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("speak: " + text);
        }
    }
}
