using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullScreenTogggle;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer audioMixer = default;

    private Resolution[] resolutions;


    public void Start()
    {
        PutResolutionValue();
        ChangeFullScreenValue();
        ChangeVolume();
    }

    // Change the resolution of the game window.
    // It takes as a parameter the index of the desired resolution in the list of available resolutions.
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // Change the full screen state of the game.
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    // Change the main volume of the game.
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("mainVolume", volume);
    }

    // Gets all available resolutions and selects current one.
    private void PutResolutionValue()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        // Converts the resolution list into a string list.
        int currentResolutionIndex = 0;
        List<string> options = new List<string>();
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width +  " x " + resolutions[i].height;
            options.Add(option);

            // Gets the current resolution index.
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // Gets the full monitor status and displays it on the UI.
    private void ChangeFullScreenValue()
    {
        fullScreenTogggle.isOn =  Screen.fullScreen;
    }

    // Gets the main volume value and displays it on the UI
    private void ChangeVolume()
    {
        float volume;
        audioMixer.GetFloat("mainVolume", out volume);
        volumeSlider.value = volume;
    }
}
