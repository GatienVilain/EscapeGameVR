using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera = default;
    [SerializeField] private GameObject pauseWindow = default;
    [SerializeField] private GameObject settingsWindow = default;

    public static bool gameIsPaused = false;

    private static bool inSettingsWindow = false;

    // Update is called once per frame
    private void Update()
    {
        if ( MenuButtonPressed() || Input.GetButtonDown("Cancel") )
        {
            if (gameIsPaused)
            {
                if (inSettingsWindow)
                {
                    CloseSettingsWindow();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Paused();
            }
        }
    }

    public void GetSettings()
    {
        settingsWindow.SetActive(true);
        inSettingsWindow = true;
    }

    // Exit the setting window.
    public void CloseSettingsWindow()
    {
        settingsWindow.SetActive(false);
        inSettingsWindow = false;
    }


    public void ResetMap()
    {
        Resume();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void MoveToMainMenu()
    {
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Resume()
    {
        pauseWindow.SetActive(false);
        Time.timeScale = 1;
        gameIsPaused = false;
    }

    public void Paused()
    {
        SetMenuPosition();
        pauseWindow.SetActive(true);
        Time.timeScale = 0;
        gameIsPaused = true;
    }

    private void SetMenuPosition()
    {
        Vector3 position = new Vector3(mainCamera.transform.forward.x, -0.1f, mainCamera.transform.forward.z);
        transform.position = mainCamera.transform.position + 2 * position;

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    private bool MenuButtonPressed()
    {
        // Get the left hand device.
        var leftHandedControllers = new List<InputDevice>();
        var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);

        // Check if the primary button is pressed of any of the left hand devices.
        foreach (var controller in leftHandedControllers)
        {
            if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
            {
                // If the primary button is pressed, don’t check the other controllers and return true.
                return true;
            }
        }
        // If the primary button is not pressed on any of the left hand devices, return false.
        return false;
    }
}
