using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public enum Sound
    {
        PlayerMove,
        PlayerPass,
        PlayerKick,
        PlayerDie,
        Background,
    }

    private static Dictionary<Sound, float> soundTimeDictionary;

    public static void Initialize()
    {
        soundTimeDictionary = new Dictionary<Sound, float>();
        soundTimeDictionary[Sound.PlayerMove] = 0f;
    }

    public static void PlaySound (Sound sound)
    {
        if (CanPlaySound(sound))
        {
            GameObject soundGameObject = new GameObject("Sound");
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(GetAudioClip(sound));
        }
    }

    public static void PlaySound (Sound sound, Vector3 position) //3D sound
    {
        if (CanPlaySound(sound))
        {
            GameObject soundGameObject = new GameObject("Sound");
            soundGameObject.transform.position = position;
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.clip = GetAudioClip(sound);
            audioSource.maxDistance = 100f;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;            
            audioSource.Play(0);
        }
    }

    public static void PlaySoundBG (Sound sound)
    {
        GameObject soundGameObject = new GameObject("SoundBg");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = GetAudioClip(sound);
        audioSource.loop = true;
        audioSource.Play(0);
    }

    private static bool CanPlaySound(Sound sound)
    {
        switch (sound)
        {
        case Sound.PlayerMove:
            if(soundTimeDictionary.ContainsKey(sound))
            {
                float lastTimePlayed = soundTimeDictionary[sound];
                float playMoveTimerMax = 1f;
                if (lastTimePlayed + playMoveTimerMax < Time.time)
                {
                    soundTimeDictionary[sound] = Time.time;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

        default: 
            return true;
        }
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }

        Debug.LogError("Sound" + sound + "not found!");
        return null;
    }
}
