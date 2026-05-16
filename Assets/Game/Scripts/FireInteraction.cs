using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireInteraction : ParticleControlTemplate
{
    [Header("Initial Setting")]
    public GameObject initialFire;
    public GameObject firePath;
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
        fire_1(true);
        yield return new WaitForSeconds(3);
        yield return StartCoroutine(MoveFirePath(firstFirePositions[0].transform.position));
        fire_1_1(true);
    }

    private IEnumerator MoveFirePath(Vector3 target)
    {
        firePath.SetActive(true);
        while (Vector3.Distance(firePath.transform.position, target) > 0.01f)
        {
            firePath.transform.position = Vector3.MoveTowards(
                firePath.transform.position, target, firePath_speed * Time.deltaTime);
            yield return null;
        }
        firePath.transform.position = target;
        firePath.SetActive(false);
    }

    private void fire_1(bool control) => PrefabControl("Fire", initialFire, control, 0);
    private void fire_1_1(bool control)
    {
        for (int i = 0; i < firstFirePositions.Length; i++)
        {
            if (!firePosHasUsed[i])
            {
                Instantiate(torchFire, firstFirePositions[i].transform.position, firstFirePositions[i].transform.rotation);
                firePosHasUsed[i] = true;
                break;
            }
        }
    }
}
