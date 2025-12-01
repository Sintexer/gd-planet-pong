using UnityEngine;

public class SquashOnMove : MonoBehaviour
{
    private const float EPSILON = 1e-4f;

    [SerializeField]
    private PadVisualMotionSource motionSource;

    [SerializeField]
    private Vector3 squashAxis = Vector3.up;

    [SerializeField]
    private float referenceSpeed = 8f;

    [SerializeField]
    private float shrinkAmount = 0.25f;

    [SerializeField]
    private float stretchAmount = 0.20f;

    [SerializeField]
    private AnimationCurve squashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private float lerpSpeed = 12f;

    private Vector3 defaultScale;
    private Vector3 currentScale;

    private void Reset()
    {
        motionSource = GetComponentInParent<PadVisualMotionSource>();
    }

    private void Awake()
    {
        defaultScale = transform.localScale;
        currentScale = defaultScale;
    }

    private void LateUpdate()
    {
        Vector2 velocity = motionSource ? motionSource.CurrentVelocity : Vector2.zero;
        float speed = velocity.magnitude;
        float normalized = Mathf.Clamp01(speed / referenceSpeed);
        float t = squashCurve.Evaluate(normalized);

        Vector3 targetScale = ComputeTargetScale(t);
        currentScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * lerpSpeed);
        transform.localScale = currentScale;
    }

    private Vector3 ComputeTargetScale(float t)
    {
        if (t <= EPSILON)
            return defaultScale;

        Vector3 axis = squashAxis.normalized; // e.g. (0,1,0) for vertical squash
        Vector3 shrinkVector = axis * (shrinkAmount * t);
        Vector3 stretchVector = Vector3.one - axis;
        stretchVector *= stretchAmount * t;

        Vector3 target = defaultScale;
        target -= Vector3.Scale(defaultScale, shrinkVector);
        target += Vector3.Scale(defaultScale, stretchVector);
        return target;
    }
}