using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SFX : MonoBehaviour
{
    [SerializeField]
    public SoundEffect[] soundEffects;
    
    private float lowPitchRange = .95f;
    
    [SerializeField]
    private float highPitchRange = 1.05f;

    [SerializeField]
    private AudioSource effectsSource;
    [SerializeField]
    private AudioSource musicSource;
    
    [SerializeField]
    public SoundEffect menuMusic;
    
    [SerializeField]
    public SoundEffect gameMusic;
    
    [SerializeField]
    public SoundEffect gameEndMusic;
    
    private Dictionary<string, AudioClip[]> sfxDictionary;
    public static SFX Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Persist across scenes

        effectsSource = GetComponent<AudioSource>();

        sfxDictionary = new Dictionary<string, AudioClip[]>();
        foreach (var sfx in soundEffects)
        {
            if (!sfxDictionary.ContainsKey(sfx.id))
            {
                sfxDictionary.Add(sfx.id, sfx.clips);
            }
            else
            {
                Debug.LogWarning(
                    $"SFXManager: Duplicate sound effect ID '{sfx.id}' found. Only the first one will be used.");
            }
        }
    }
    
    public void Play(string sfxID, float volumeMultiplier = 1f)
    {
        if (sfxDictionary.TryGetValue(sfxID, out AudioClip[] clips))
        {
            SoundEffect sfx = Array.Find(soundEffects, se => se.id == sfxID);
            if (sfx != null)
            {
                float randomPitch = Random.Range(lowPitchRange, highPitchRange);
                effectsSource.pitch = randomPitch;
                var clip = clips[Random.Range(0, clips.Length)];
                effectsSource.PlayOneShot(clip, sfx.volume * volumeMultiplier);
            }
        }
        else
        {
            Debug.LogWarning($"SFXManager: Sound effect with ID '{sfxID}' not found.");
        }
    }

    public void PlayLooping(bool isMenuMusic, bool isGameOver = false, float volumeMultiplier = 1f)
    {
        StopLoopingSFX();
        var music = isMenuMusic ? menuMusic : gameMusic;
        if (isGameOver)
        {
            music = gameEndMusic;
        }
        var sound = music.clips[Random.Range(0, music.clips.Length)];
            musicSource.clip = sound;
            musicSource.loop = true;
            musicSource.volume = music.volume * volumeMultiplier;
            musicSource.Play();
    }

    
    public void StopLoopingSFX()
    {
        musicSource.Stop();
        musicSource.loop = false;
        musicSource.clip = null;
    }

    [Serializable]
    public class SoundEffect
    {
        public string id;
        public AudioClip[] clips;

        [Range(0f, 1f)]
        public float volume = 1f;
    }
}