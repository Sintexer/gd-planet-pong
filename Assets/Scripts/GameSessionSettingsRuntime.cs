using UnityEngine;

public class GameSessionSettingsRuntime : MonoBehaviour
{
    [SerializeField]
    private GameSessionSettings sourceAsset;

    public static GameSessionSettings Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = Instantiate(sourceAsset); // runtime clone
        Instance.ResetToDefaults();

        DontDestroyOnLoad(gameObject);
    }
}