using System.Collections;
using UnityEngine;

public class PaddleIntroSpawn : MonoBehaviour
{
    [SerializeField]
    private Transform paddle;

    [SerializeField]
    private Vector3 offscreenOffset = new(-16f, 0f, 0f);

    [SerializeField]
    private float duration = 0.8f;
    
    public float Duration => duration;

    [SerializeField]
    private AnimationCurve introCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 restPosition;
    private Coroutine currentRoutine;

    private void Awake()
    {
        restPosition = paddle.position;
    }

    public void PlayIntro()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        Vector3 start = restPosition + offscreenOffset;
        paddle.position = start;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = introCurve.Evaluate(t);
            paddle.position = Vector3.LerpUnclamped(start, restPosition, eased);
            yield return null;
        }

        paddle.position = restPosition;
        currentRoutine = null;
    }
}