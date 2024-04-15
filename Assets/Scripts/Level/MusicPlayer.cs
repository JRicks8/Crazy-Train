using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public enum Sound
    {
        Song_EnemiesClosingIn,
        Song_Menu,
        Sound_TrainNoise,
    }

    public static MusicPlayer instance;

    public AudioClip[] audioClips;
    public List<AudioSource> tracks;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Two music players found in the same scene.");
            Destroy(gameObject);
            return;
        }

        instance = this;

        for (int i = 0; i < audioClips.Length; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClips[i];

            tracks.Add(audioSource);
        }
    }

    public void PlaySound(Sound sound, bool loop, float volume = 1.0f)
    {
        tracks[(int)sound].loop = loop;
        tracks[(int)sound].volume = volume;
        tracks[(int)sound].Play();
    }

    public void PlaySoundOneShot(Sound sound, float volumeScale = 1.0f)
    {
        tracks[(int)sound].PlayOneShot(audioClips[(int)sound], volumeScale);
    }

    public void StopSound(Sound sound)
    {
        tracks[(int)sound].Stop();
    }

    public void PlaySoundFadeIn(Sound sound, float fadeInTime, bool loop, float targetVolume = 1.0f)
    {
        tracks[(int)sound].loop = loop;
        StartCoroutine(SoundFadeInHandler(sound, fadeInTime, targetVolume));
    }

    private IEnumerator SoundFadeInHandler(Sound sound, float fadeInTime, float targetVolume)
    {
        tracks[(int)sound].volume = 0.0f;
        tracks[(int)sound].Play();

        float timer = 0.0f;
        while (timer < fadeInTime)
        {
            timer += Time.fixedDeltaTime;

            tracks[(int)sound].volume = timer / fadeInTime * targetVolume;

            yield return new WaitForFixedUpdate();
        }

        tracks[(int)sound].volume = targetVolume;
    }

    public void PlaySoundFadeOut(Sound sound, float fadeOutTime, bool loop, float startVolume = 1.0f)
    {
        tracks[(int)sound].loop = loop;
        StartCoroutine(SoundFadeOutHandler(sound, fadeOutTime, startVolume));
    }

    private IEnumerator SoundFadeOutHandler(Sound sound, float fadeOutTime, float startVolume)
    {
        tracks[(int)sound].volume = startVolume;
        tracks[(int)sound].Play();

        float timer = 0.0f;
        while (timer < fadeOutTime)
        {
            timer += Time.fixedDeltaTime;

            tracks[(int)sound].volume = (1 - timer / fadeOutTime) * startVolume;

            yield return new WaitForFixedUpdate();
        }

        tracks[(int)sound].volume = 0.0f;
    }
}
