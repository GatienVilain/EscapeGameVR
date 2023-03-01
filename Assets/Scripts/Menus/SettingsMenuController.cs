using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;
    [SerializeField] private AudioMixer audioMixer = default;

    // private enum MixerGroups {
    //     Master = "mainVolume",
    //     Music = "MusicVolume",
    //     Sound = "SoundVolume"
    // }

    public void Start()
    {
        ChangeMainVolume();
        ChangeMusicVolume();
        ChangeSoundVolume();
    }

    // Change the main volume of the game.
    public void SetMainVolume(float volume)
    {
        audioMixer.SetFloat("mainVolume", volume);
    }

    // Change the music volume of the game.
    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", volume);
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

    // Gets the main volume value and displays it on the UI
    private void ChangeMusicVolume()
    {
        float volume;
        audioMixer.GetFloat("musicVolume", out volume);
        musicVolumeSlider.value = volume;
    }

    // Gets the main volume value and displays it on the UI
    private void ChangeSoundVolume()
    {
        float volume;
        audioMixer.GetFloat("soundVolume", out volume);
        soundVolumeSlider.value = volume;
    }
}
