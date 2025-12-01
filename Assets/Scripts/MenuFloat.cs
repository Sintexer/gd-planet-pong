using UnityEngine;

public class MenuFloat : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("World-space offset from the start position to the far point.")]
    [SerializeField]
    private Vector3 displacement = new Vector3(0f, 2f, 0f);

    [Tooltip("Seconds for a full there-and-back cycle.")]
    [SerializeField]
    private float cycleDuration = 2f;

    [Header("Feel")]
    [Tooltip("Shape of the motion. 0→1 is start→end, then it reverses.")]
    [SerializeField]
    private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 startPosition;
    private float timer;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (cycleDuration <= 0f)
            return;

        timer += Time.fixedDeltaTime;

        float halfCycle = cycleDuration * 0.5f;
        float normalized = Mathf.PingPong(timer / halfCycle, 1f);

        // Apply easing to get smooth acceleration/deceleration.
        float eased = easing.Evaluate(normalized);

        // Lerp between start and target, then back again.
        transform.position = Vector3.Lerp(
            startPosition,
            startPosition + displacement,
            eased
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 from = Application.isPlaying ? startPosition : transform.position;
        Vector3 to = from + displacement;
        Gizmos.DrawWireSphere(from, 0.05f);
        Gizmos.DrawWireSphere(to, 0.05f);
        Gizmos.DrawLine(from, to);
    }
#endif
}