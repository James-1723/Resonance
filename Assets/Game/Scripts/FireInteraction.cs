using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireInteraction : ParticleControlTemplate
{
    [Header("Initial Setting")]
    public GameObject initialFire;
    public GameObject firePath;
    public GameObject fireIndicator;
    public float firePath_speed = 1f;
    public GameObject torch;
    public GameObject torchFire;
    public GameObject[] firstFirePositions;
    private bool[] firePosHasUsed = new bool[4];

    void Start()
    {
        for (int i = 0; i < firstFirePositions.Length; i++)
            firePosHasUsed[i] = false;
        StartCoroutine(GameFlow());
    }

    void Update() { }

    IEnumerator GameFlow()
    {
        Debug.Log("InitialFire is on");
        fire_1(true);
        yield return new WaitForSeconds(3);
        Debug.Log("firePath is on");
        firePath.SetActive(true);
        fireIndicator.SetActive(true);
        fireIndicator.transform.position = initialFire.transform.position;
        yield return StartCoroutine(MoveFirePath(firstFirePositions[0].transform.position));
        fire_1_1(true);
    }

    private IEnumerator MoveFirePath(Vector3 target)
    {
        Debug.Log("start to move");
        while (Vector3.Distance(fireIndicator.transform.position, target) > 0.01f)
        {
            fireIndicator.transform.position = Vector3.MoveTowards(
                fireIndicator.transform.position, target, firePath_speed * Time.deltaTime);
            yield return null;
        }
        fireIndicator.transform.position = target;
        // firePath.SetActive(false);
    }

    private void fire_1(bool control) => PrefabControl("Fire", initialFire, control, 0);
    private void fire_1_1(bool control)
    {
        for (int i = 0; i < firstFirePositions.Length; i++)
        {
            if (!firePosHasUsed[i])
            {
                Instantiate(initialFire, firstFirePositions[i].transform.position, firstFirePositions[i].transform.rotation);
                firePosHasUsed[i] = true;
                break;
            }
        }
    }
}