using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadRecoil : MonoBehaviour
{
    [SerializeField]
    private float hitStopDuration = 0.02f;
    
    [Header("Timing")]
    [Tooltip("Total duration of the recoil (to the peak and back).")]
    [SerializeField]
    private float recoilTime = 0.12f;

    [Tooltip("How far the paddle visuals push back (in units).")]
    [SerializeField]
    private float recoilDistance = 0.12f;

    [Header("Feel")]
    [Tooltip("Shape of the motion (0 → rest, 0.5 → peak, 1 → rest).")]
    [SerializeField]
    private AnimationCurve recoilCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    [Tooltip("Scale the recoil by the incoming ball speed.")]
    [SerializeField]
    private bool scaleByBallSpeed = true;

    [SerializeField]
    private float speedToDistance = 0.02f;
    
    [Tooltip("Neglect some initial speed and start applying from offset")]
    [SerializeField]
    private float speedOffset = 4f;

    [SerializeField]
    private float maxScaledDistance = 0.25f;

    private Vector3 localRest;
    private Coroutine recoilRoutine;

    private void Awake()
    {
        localRest = transform.localPosition;
    }

    public void HandleBallCollision(Collision2D ball)
    {
        Debug.Log("Handling recoil");
        var normal = ball.GetContact(0).normal; // points from ball toward paddle

        float distance = recoilDistance;
        if (scaleByBallSpeed)
        {
            float speed = ball.relativeVelocity.magnitude - speedOffset;
            distance += Mathf.Clamp(speed * speedToDistance, 0f, maxScaledDistance);
        }

        StartRecoil(-normal, distance);
    }
    private void StartRecoil(Vector2 direction, float distance)
    {
        localRest = transform.localPosition;
        direction.Normalize();

        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);
        }

        recoilRoutine = StartCoroutine(RecoilRoutine(direction, distance));
    }

    private IEnumerator RecoilRoutine(Vector2 direction, float distance)
    {
        float targetRecoilTime = recoilTime + 0.01f *(distance / recoilDistance); // scale time by impact
        // float impactMagnitude = targetRecoilTime / recoilTime;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1.0f;

        float timer = 0f;
        while (timer < targetRecoilTime)
        {
            float t = Mathf.Clamp01(timer / targetRecoilTime);
            float curveValue = recoilCurve.Evaluate(t);

            transform.localPosition = localRest + (Vector3)(direction * (distance * curveValue));

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = localRest;
        recoilRoutine = null;
    }
}