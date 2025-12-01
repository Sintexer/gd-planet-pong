using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    [Tooltip("Shape of the motion (0 → rest, 0.5 → peak, 1 → rest).")]
    [SerializeField]
    private AnimationCurve recoilCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
    
    private Coroutine shakeCo = null;

    public void Shake(float duration, float magnitude)
    {
        if (shakeCo != null)
        {
            StopCoroutine(shakeCo);
        }

        StartCoroutine(ShakeRoutine(duration, magnitude));
    }
    
public IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float timeDamping = recoilCurve.Evaluate(t);
            float x = Random.Range(-1f, 1f) * magnitude * timeDamping;
            float y = Random.Range(-1f, 1f) * magnitude * timeDamping;

            // Keep the Z position (usually -10) intact!
            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        transform.localPosition = originalPos;
    }

}
