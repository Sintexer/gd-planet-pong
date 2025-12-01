using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    private static readonly int Start = Animator.StringToHash("start");

    [SerializeField]
    private Animator transition;

    [SerializeField]
    private float transitionDuration;

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        transition.SetTrigger(Start);
        yield return new WaitForSeconds(transitionDuration);
        SceneManager.LoadScene(sceneName);
    }
}
