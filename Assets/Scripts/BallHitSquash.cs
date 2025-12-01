using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BallHitSquash : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform squashPivot;

    [SerializeField]
    private Rigidbody2D rb;

    [Header("Shape")]
    [SerializeField, Range(0f, 0.6f)]
    private float maxSquash = 0.25f;

    [SerializeField, Range(0f, 1.5f)]
    private float stretchMultiplier = 0.5f;

    [SerializeField]
    private float squashDuration = 0.18f;

    [SerializeField]
    private AnimationCurve squashCurve = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(0.45f, 1f, 0f, 0f),
        new Keyframe(1f, 0f, -2f, 0f)
    ); // 0 → 1 → 0 shape

    [Header("Strength scaling")]
    [SerializeField]
    private float referenceImpactSpeed = 14f;

    [SerializeField]
    private float maxStrengthMultiplier = 1.5f;

    private static readonly float EPSILON = 1e-4f;

    private Vector3 defaultScale;
    private Quaternion defaultLocalRotation;
    private Coroutine squashRoutine;

    private void Reset()
    {
        squashPivot = transform;
        rb = GetComponentInParent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (!squashPivot) squashPivot = transform;
        if (!rb) rb = GetComponentInParent<Rigidbody2D>();

        defaultScale = squashPivot.localScale;
        defaultLocalRotation = squashPivot.localRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 << collision.collider.gameObject.layer == 0)
            return;

        var contact = collision.GetContact(0);
        Vector2 worldDirection = -contact.normal;
        float speed = collision.relativeVelocity.magnitude;
        float strength = Mathf.Clamp(speed / referenceImpactSpeed, 0.2f, maxStrengthMultiplier);

        TriggerSquash(worldDirection, strength);
    }

    public void TriggerSquash(Vector2 worldDirection, float strength = 1f)
    {
        if (squashRoutine != null)
            StopCoroutine(squashRoutine);

        squashRoutine = StartCoroutine(SquashRoutine(worldDirection, strength));
    }

    private IEnumerator SquashRoutine(Vector2 worldDirection, float strength)
    {
        if (worldDirection.sqrMagnitude < EPSILON)
            worldDirection = Vector2.up;

        Transform parent = squashPivot.parent;
        Vector3 directionLocal = parent
            ? parent.InverseTransformDirection(new Vector3(worldDirection.x, worldDirection.y, 0f))
            : new Vector3(worldDirection.x, worldDirection.y, 0f);

        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, directionLocal.normalized);
        strength = Mathf.Clamp(strength, 0f, maxStrengthMultiplier);

        float elapsed = 0f;
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / squashDuration);
            float curve = squashCurve.Evaluate(t);
            float squashAmount = maxSquash * curve * strength;
            float stretchAmount = squashAmount * stretchMultiplier;

            squashPivot.localRotation = Quaternion.Slerp(defaultLocalRotation, targetRotation, curve);

            Vector3 scale = defaultScale;
            scale.y *= 1f - squashAmount; // along impact axis
            scale.x *= 1f + stretchAmount; // perpendicular axes
            scale.z *= 1f + stretchAmount;

            squashPivot.localScale = scale;
            yield return null;
        }

        squashPivot.localScale = defaultScale;
        squashPivot.localRotation = defaultLocalRotation;
        squashRoutine = null;
    }
}