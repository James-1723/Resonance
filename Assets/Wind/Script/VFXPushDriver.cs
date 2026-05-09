using UnityEngine;
using UnityEngine.VFX;

public class VFXMoveDirectionSender : MonoBehaviour
{
    public VisualEffect vfx;
    public Transform target;

    public bool Road = false;

    private Vector3 lastPosition;
    private float roadWidth = 0f;
    private bool lastRoad = false;

    void Start()
    {
        if (target == null)
            target = transform;

        lastPosition = target.position;

        if (vfx != null)
        {
            vfx.SetFloat("RoadWidth", 0f);
        }
    }

    void Update()
    {
        if (vfx == null || target == null) return;

        Vector3 delta = target.position - lastPosition;

        if (delta.sqrMagnitude > 0.000001f)
        {
            Vector3 moveDirection = delta.normalized;
            vfx.SetVector3("MoveDirection", moveDirection);
        }

        if (Road && !lastRoad)
        {
            roadWidth = 0f;
        }

        if (Road)
        {
            roadWidth += Time.deltaTime;
            roadWidth = Mathf.Clamp(roadWidth, 0f, 10f);
            vfx.SetFloat("RoadWidth", roadWidth);
        }

        lastRoad = Road;
        lastPosition = target.position;
    }
}