using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource backgroundSource;
    public AudioSource bossEnterStinger;

    [Header("Audio Tracks")]
    public AudioClip menuTrack;
    public AudioClip hubTrack;
    public AudioClip levelTrack;
    public AudioClip bossFight;


    [HideInInspector] public List<AudioSource> sfxSources;

    private float musicVol;
    private float sfxVol;

    public float MusicVol { get { return musicVol; } }
    public float SFXVol { get { return sfxVol; } }

    private void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Debug.Log("Audio Manager instance already exists");
            return;
        }
        instance = this;
        #endregion
    }

    public void Start()
    {
        sfxSources = new List<AudioSource>
        {
            bossEnterStinger
        };

        musicVol = .65f;
        sfxVol = .65f;
    }

    public void ChangeMusicVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1);

        backgroundSource.volume =  musicVol = volume;
    }

    public void ChangeSFXVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1);
        sfxVol = volume;

        foreach (AudioSource source in sfxSources)
        {
            source.volume = volume;
        }
    }

    public void PlayMenuTrack()
    {
        backgroundSource.clip = menuTrack;
        backgroundSource.Play();
    }

    public void PlayHubTrack()
    {
        backgroundSource.clip = hubTrack;
        backgroundSource.Play();
    }

    public void PlayLevelTrack()
    {
        backgroundSource.clip = levelTrack;
        backgroundSource.Play();
    }

    public void PlayBossFightAudio()
    {
        bossEnterStinger.Play();
        backgroundSource.clip = bossFight;
        backgroundSource.Play();
    }

    public void StopMusic()
    {
        backgroundSource.Stop();
    }
}


