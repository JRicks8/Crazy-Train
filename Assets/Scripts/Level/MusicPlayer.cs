using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public enum Sound
    {
        Song_BossLayer,
        Song_EnemiesClosingIn,
        Song_Menu,
        Sound_CoinCollect1,
        Sound_CoinCollect2,
        Sound_CrowDie,
        Sound_CrowOuch1,
        Sound_CrowOuch2,
        Sound_DeathFalling,
        Sound_GunnerDie,
        Sound_GunnerOuch1,
        Sound_GunnerOuch2,
        Sound_Gunshot1,
        Sound_Gunshot2,
        Sound_ItemCollect,
        Sound_Jump,
        Sound_Land,
        Sound_Ouch,
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
            audioSource.playOnAwake = false;

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

    public void SetVolume(Sound sound, float volume)
    {
        tracks[(int)sound].volume = volume;
    }

    public void PlaySoundFadeIn(Sound sound, float duration, bool loop, float targetVolume = 1.0f)
    {
        tracks[(int)sound].loop = loop;
        StartCoroutine(SoundFadeInHandler(sound, duration, targetVolume));
    }

    private IEnumerator SoundFadeInHandler(Sound sound, float duration, float targetVolume)
    {
        tracks[(int)sound].volume = 0.0f;
        tracks[(int)sound].Play();

        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;

            tracks[(int)sound].volume = timer / duration * targetVolume;

            yield return new WaitForFixedUpdate();
        }

        tracks[(int)sound].volume = targetVolume;
    }

    public void StopSoundFadeOut(Sound sound, float duration, float startVolume = 1.0f)
    {
        StartCoroutine(SoundFadeOutHandler(sound, duration, startVolume));
    }

    private IEnumerator SoundFadeOutHandler(Sound sound, float duration, float startVolume)
    {
        tracks[(int)sound].volume = startVolume;

        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;

            tracks[(int)sound].volume = (1 - timer / duration) * startVolume;

            yield return new WaitForFixedUpdate();
        }

        tracks[(int)sound].Stop();
    }

    public void ChangeVolumeGradual(Sound sound, float targetVolume, float duration)
    {
        StartCoroutine(ChangeVolumeGradualHandler(sound, targetVolume, duration));
    }

    private IEnumerator ChangeVolumeGradualHandler(Sound sound, float targetVolume, float duration)
    {
        float startVolume = tracks[(int)sound].volume;
        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;

            tracks[(int)sound].volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);

            yield return new WaitForFixedUpdate();
        }
    }
}
