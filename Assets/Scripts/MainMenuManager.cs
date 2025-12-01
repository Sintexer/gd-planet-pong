using System;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField]
    private TransitionController transitionController;

    public void Start()
    {
        SFX.Instance.PlayLooping(true);
    }

    public void StartPlayerVsBotGame()
    {
        SFX.Instance.Play("click");
        transitionController.ChangeScene("PlayerVsBot");
    }
    
    public void StartPlayerVsPlayerGame()
    {
        SFX.Instance.Play("click");
        transitionController.ChangeScene("PlayerVsPlayer");
    }
}
