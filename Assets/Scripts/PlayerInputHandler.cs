using System;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    
    private PlayerInputSet _playerInputSet;
    private PadLogic padLogic;

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private bool player2;

    private void Awake()
    {
        padLogic = GetComponent<PadLogic>();
        _playerInputSet = new PlayerInputSet();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        _playerInputSet.Enable();
        if (!player2)
        {
            _playerInputSet.Player.Move.started += ctx =>
            {
                var input = ctx.ReadValue<Vector2>();
                padLogic.Move(Mathf.RoundToInt(input.y));
            };
            _playerInputSet.Player.Move.canceled += _ => padLogic.Stop();

            _playerInputSet.Player.Pause.performed += _ => gameManager.TogglePause();
        }
        else
        {
            _playerInputSet.Player2.Move.started += ctx =>
            {
                var input = ctx.ReadValue<Vector2>();
                padLogic.Move(Mathf.RoundToInt(input.y));
            };
            _playerInputSet.Player2.Move.canceled += _ => padLogic.Stop();
        }
    }

    public void TurnOff()
    {
        OnDisable();
    }

    private void OnDisable()
    {
        _playerInputSet.Disable();
    }
}
