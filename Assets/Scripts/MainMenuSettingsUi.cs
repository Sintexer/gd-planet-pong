using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainMenuSettingsUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown difficultyDropdown;

    private void Start()
    {
        var settings = GameSessionSettingsRuntime.Instance;

        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(Enum.GetNames(typeof(AiDifficulty)).ToList());
        difficultyDropdown.value = (int)settings.aiDifficulty;
        difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
    }

    private void OnDifficultyChanged(int index)
    {
        GameSessionSettingsRuntime.Instance.aiDifficulty = (AiDifficulty)index;
    }
}