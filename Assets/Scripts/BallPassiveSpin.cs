using UnityEngine;

[DisallowMultipleComponent]
public class BallPassiveSpin : MonoBehaviour
{
    [Header("Spin settings")]
    [SerializeField] private float baseSpinSpeed = 180f;       // degrees per second
    [SerializeField] private float damping = 2f;               // how quickly we ease back to base
    [SerializeField] private float maxSpinSpeed = 540f;
    [SerializeField] private float minSpinSpeed = -540f;

    [Header("Per-hit impulse")]
    [SerializeField] private Vector2 spinImpulseRange = new(-120f, 120f);

    private float currentSpinSpeed;
    private Quaternion accumulatedRotation = Quaternion.identity;

    private void Awake()
    {
        currentSpinSpeed = baseSpinSpeed;
    }

    private void LateUpdate()
    {
        // Ease back toward the base spin speed.
        currentSpinSpeed = Mathf.Lerp(currentSpinSpeed, baseSpinSpeed, damping * Time.deltaTime);
        currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, minSpinSpeed, maxSpinSpeed);

        float deltaAngle = currentSpinSpeed * Time.deltaTime;
        accumulatedRotation *= Quaternion.Euler(0f, 0f, deltaAngle);
        transform.localRotation = accumulatedRotation;
    }

    public void AddRandomImpulse()
    {
        AddSpinImpulse(Random.Range(spinImpulseRange.x, spinImpulseRange.y));
    }

    public void AddSpinImpulse(float deltaSpeed)
    {
        currentSpinSpeed += deltaSpeed;
        currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, minSpinSpeed, maxSpinSpeed);
    }
}
