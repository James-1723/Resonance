using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FireInteraction : ParticleControlTemplate
{
    [Header("Initial Setting")]
    public GameObject hand;
    public GameObject fireBackground;
    // public GameObject initialFire;
    public GameObject handFire;
    public GameObject firePath;
    public GameObject fireIndicator;
    public float firePath_speed = 1f;
    public GameObject[] firstFirePositions;
    public GameObject[] secondFirePositions;

    [Header("Wait")]
    public float wait_1 = 5;
    public float wait_2 = 20;
    public float wait_3 = 20;
    public float wait_4 = 20;
    private bool isBurning = false;

    void Start()
    {
        Debug.Log("Start Fire Interaction");
        StartCoroutine(GameFlow());
    }

    void Update()
    {

    }

    IEnumerator GameFlow()
    {
        audioController.PlayAudio("Fire", 0);
        yield return new WaitForSeconds(3f);
        Fire_1(true);
        audioController.PlayAudio("Fire", 1);
        yield return new WaitForSeconds(wait_1);
        Fire_1(false);
        Fire_2(true);
        audioController.PlayAudio("Fire", 2);
        yield return new WaitForSeconds(wait_2);
        Fire_2(false);
        Fire_3(true);
        audioController.PlayAudio("Fire", 3);
        yield return new WaitForSeconds(wait_3);
        // Fire_3(false);
        audioController.PlayAudio("Fire", 4);
        yield return StartCoroutine(Fire_4(wait_4));
        audioController.StopAudio("Fire");

        handFire.SetActive(false);
        firePath.SetActive(false);
        for (int i = 0; i < 5; i++)
        {
            firstFirePositions[i].SetActive(false);
            secondFirePositions[i].SetActive(false);
        }
        gamemanager.AddCharToHistory('燃');
        gamemanager.AddCharToHistory('暖');
        gamemanager.AddCharToHistory('擴');
        gamemanager.gameOver = true;
        gamemanager.NotifyPlayerDone();
    }

    private void Fire_1(bool control)
    {
        fireBackground.SetActive(control);
    }

    private void Fire_2(bool control)
    {
        // initialFire.SetActive(control);
        firePath.SetActive(control);
    }

    private Coroutine _fire3Coroutine;

    private void Fire_3(bool control)
    {
        handFire.SetActive(control);
        foreach (var pos in secondFirePositions)
            pos.SetActive(control);

        if (control)
        {
            _fire3Coroutine = StartCoroutine(Fire_3ProximityCheck());
        }
        else if (_fire3Coroutine != null)
        {
            StopCoroutine(_fire3Coroutine);
        }
    }

    private IEnumerator Fire_3ProximityCheck()
    {
        while (true)
        {
            foreach (var pos in secondFirePositions)
            {
                if (Vector3.Distance(hand.transform.position, pos.transform.position) < 0.2f)
                {
                    Transform fireChild = pos.transform.Find("PointFire");
                    if (fireChild != null && !fireChild.gameObject.activeSelf)
                        fireChild.gameObject.SetActive(true);
                    audioController.PlayAudio("SoundEffect", 11);
                }
            }
            yield return null;
        }
    }

    private IEnumerator Fire_4(float duration)
    {
        Debug.Log("Start Waiting");
        // 啟動最近的 initialFire
        int nearestIdx = GetNearestFireIndex(player_head.transform.position);
        firstFirePositions[nearestIdx].SetActive(true);
        if (isBurning == false) 
        {
            audioController.PlayAudio("SoundEffect", 10);
            isBurning = true;
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 lastHeadPos = player_head.transform.position;
        float elapsed = 0.5f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;

            Vector3 currentHeadPos = player_head.transform.position;
            Vector3 displacement = currentHeadPos - lastHeadPos;
            lastHeadPos = currentHeadPos;

            if (displacement.magnitude < 0.001f)
                continue;

            int targetIdx = GetMostApproachedFireIndex(currentHeadPos, displacement);

            if (!firstFirePositions[targetIdx].activeSelf)
            {
                firstFirePositions[targetIdx].SetActive(true);
            }
            else
            {
                // 已有火焰，增加 FlameSize
                VisualEffect vfx = firstFirePositions[targetIdx]
                    .transform.Find("fire")
                    ?.GetComponentInChildren<VisualEffect>();

                if (vfx != null)
                    vfx.SetFloat("FlameSize", vfx.GetFloat("FlameSize") + 0.2f);
            }
        }
    }

    private int GetNearestFireIndex(Vector3 position)
    {
        int nearest = 0;
        float minDist = float.MaxValue;
        for (int i = 0; i < firstFirePositions.Length; i++)
        {
            float dist = Vector3.Distance(position, firstFirePositions[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }
        return nearest;
    }

    private int GetMostApproachedFireIndex(Vector3 playerPos, Vector3 displacement)
    {
        int bestIdx = 0;
        float maxDot = float.MinValue;
        Vector3 moveDir = displacement.normalized;
        for (int i = 0; i < firstFirePositions.Length; i++)
        {
            Vector3 toFire = (firstFirePositions[i].transform.position - playerPos).normalized;
            float dot = Vector3.Dot(moveDir, toFire);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestIdx = i;
            }
        }
        return bestIdx;
    }
}