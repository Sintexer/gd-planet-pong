using UnityEngine;

public class PadVisualMotionSource : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private Transform recoilPivot;

    private Vector3 lastPivotWorldPos;

    public Vector2 CurrentVelocity { get; private set; }

    private void Awake()
    {
        if (!rb) rb = GetComponentInParent<Rigidbody2D>();
        if (!recoilPivot) recoilPivot = transform;

        lastPivotWorldPos = recoilPivot.position;
    }

    private void LateUpdate()
    {
        // Base velocity from the rigidbody (player input, physics)
        Vector2 baseVelocity = rb ? rb.linearVelocity : Vector2.zero;

        // Additional “visual” velocity from recoil offset
        Vector3 pivotWorldPos = recoilPivot.position;
        var deltaTime = Time.deltaTime;
        if (deltaTime != 0)
        {
            Vector3 visualVelocity = (pivotWorldPos - lastPivotWorldPos) / deltaTime;
            lastPivotWorldPos = pivotWorldPos;
            CurrentVelocity = baseVelocity + (Vector2)visualVelocity;
        }
    }
}