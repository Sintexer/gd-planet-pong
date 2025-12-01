using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Game Session Settings")]
public class GameSessionSettings : ScriptableObject
{
    [Header("Defaults")]
    [SerializeField]
    private AiDifficulty defaultAiDifficulty = AiDifficulty.Normal;

    [SerializeField]
    public Vector2 normalInitSpeed;
    
    [SerializeField]
    public Vector2 hardInitSpeed;
    
    [SerializeField]
    public float targetScore;

    [SerializeField]
    public float padSpeed;

    [System.NonSerialized]
    public AiDifficulty aiDifficulty;

    private void OnEnable() => ResetToDefaults();

    public void ResetToDefaults()
    {
        Debug.Log("enabling and resetting settings");
        aiDifficulty = defaultAiDifficulty;
    }
}