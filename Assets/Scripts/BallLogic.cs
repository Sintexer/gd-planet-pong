using UnityEngine;

public class BallLogic : MonoBehaviour
{
    [SerializeField]
    private float hitShakeMultiplier = 0.2f;

    [SerializeField]
    private float padHitShakeDuration = 0.1f;

    [SerializeField]
    private float goalShakeDuration = 0.4f;

    [SerializeField]
    private BallPassiveSpin spin;

    private Rigidbody2D rb;
    private CameraShake cameraShake;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Pad")) return;
        ShakeScreen(padHitShakeDuration);
        spin.AddRandomImpulse();
    }

    public void ShakeScreen()
    {
        ShakeScreen(goalShakeDuration);
    }

    public void ShakeScreen(float duration)
    {
        var magnitude = Mathf.Clamp(rb.linearVelocity.magnitude * hitShakeMultiplier, 0f, 1f);
        Debug.Log($"shake {magnitude}");
        cameraShake.Shake(duration, magnitude);
    }
}