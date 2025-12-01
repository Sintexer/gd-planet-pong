    using System.Collections;
    using UnityEngine;

    public class ExitGameScript : MonoBehaviour
    {
        private bool exited;
        
        public void ExitGame()
        {
            if (!exited)
            {
                StartCoroutine(ExitRoutine());
                exited = true;
            }
        }

        private IEnumerator ExitRoutine()
        {
            SFX.Instance.Play("click_exit");
            yield return new WaitForSeconds(1f); // play sound
            Application.Quit();
            Debug.Log("Exiting game...");
        }
    }