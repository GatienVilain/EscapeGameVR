using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Audio Menu")]
    [SerializeField] private AudioMixer audioMixer = default;
    [SerializeField] private SavedInfoSettings savedInfoSettings = default;
    [Space(10)]
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;

    private bool isSettingVolume = false;

    // private enum MixerGroups {
    //     Master = "mainVolume",
    //     Music = "MusicVolume",
    //     Sound = "SoundVolume"
    // }

    public void Start()
    {
        ChangeMainVolume();
        ChangeMute();
        ChangeMusicVolume();
        ChangeSoundVolume();
    }

    // Change the main volume of the game.
    public void SetMainVolume(float volume)
    {
        if (!isSettingVolume)
        {
            isSettingVolume = true;
            muteToggle.isOn = false;
            isSettingVolume = false;
            audioMixer.SetFloat("mainVolume", volume);
        }
    }

    public void Mute()
    {
        if (!isSettingVolume)
        {
            if (muteToggle.isOn)
            {
                // Sauvegarde l’état du volume avant de le couper
                savedInfoSettings.MainVolume = mainVolumeSlider.value;
                savedInfoSettings.MuteState = true;

                // Coupe le volume sur le mixer et sur l’UI
                audioMixer.SetFloat("mainVolume", -80);
                isSettingVolume = true;
                mainVolumeSlider.value = -80;
                isSettingVolume = false;
            }
            else
            {
                savedInfoSettings.MuteState = false;

                // Remet le volume à la valeur sauvegardée
                mainVolumeSlider.value = savedInfoSettings.MainVolume;
            }
        }
    }

    // Change the music volume of the game.
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", volume - 10);
    }

    // Change the sound volume of the game.
    public void SetSoundVolume(float volume)
    {
        audioMixer.SetFloat("soundVolume", volume);
    }

    // Gets the main volume value and displays it on the UI
    private void ChangeMainVolume()
    {
        float volume;
        audioMixer.GetFloat("mainVolume", out volume);
        mainVolumeSlider.value = volume;
    }

    // Gets mute state and displays it on the UI
    private void ChangeMute()
    {
        if (savedInfoSettings.MuteState)
        {
            float tmp = savedInfoSettings.MainVolume;
            muteToggle.isOn = true;
            savedInfoSettings.MainVolume = tmp;
        }
        else
        {
            isSettingVolume = true;
            muteToggle.isOn = false;
            isSettingVolume = false;
        }
    }

    // Gets the main volume value and displays it on the UI
    private void ChangeMusicVolume()
    {
        float volume;
        audioMixer.GetFloat("musicVolume", out volume);
        musicVolumeSlider.value = volume + 10;
    }

    // Gets the main volume value and displays it on the UI
    private void ChangeSoundVolume()
    {
        float volume;
        audioMixer.GetFloat("soundVolume", out volume);
        soundVolumeSlider.value = volume;
    }
}
