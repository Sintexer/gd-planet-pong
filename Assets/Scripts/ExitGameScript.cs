using System.Collections;
using UnityEngine;

public class ExitGameScript : MonoBehaviour
{
    [SerializeField] private float exitDelay = 0.8f;   // how long the click SFX lasts
    private bool isQuitting;

    [SerializeField]
    private GameObject browserExitTipPanel;

    public void ExitGame()
    {
        if (isQuitting) return;
        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
    {
        isQuitting = true;

        if (SFX.Instance) SFX.Instance.Play("click_exit");

        // Wait in real time so it works even if timeScale = 0 (pause menu)
        yield return new WaitForSecondsRealtime(exitDelay);

        QuitPlatformSafe();
    }

    private void QuitPlatformSafe()
    {
        Debug.Log("Exiting game...");

// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPlaying = false;
// #endif
        browserExitTipPanel.SetActive(true);
        Application.Quit();
    }
}
