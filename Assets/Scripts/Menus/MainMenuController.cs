using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject levelWindow;
    [SerializeField] private GameObject settingsWindow;

    // This variable is used to check if the settings window is open.
    private static bool inSettingsWindow = false;
     // This variable is used to check if the primary button of the left hand controller is pressed.
    private static bool menuButtonIsPressed = false;

    void Start()
    {
        StartCoroutine(SetMenu());
    }

    private void Update()
    {
        if ( MenuButtonPressed() || Input.GetButtonDown("Cancel") )
        {
            if (inSettingsWindow)
            {
                CloseSettingsWindow();
            }
        }
    }

    // Set the menu in front of the camera and rotate it to face the camera.
    private IEnumerator SetMenu()
    {
        yield return new WaitForSeconds(1);
        Vector3 position = new Vector3(mainCamera.transform.forward.x, -0.1f, mainCamera.transform.forward.z);
        transform.position = mainCamera.transform.position + 2 * position;

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void GetLevelsWindow()
    {
        levelWindow.SetActive(true);
    }

    public void LoadLevel(string levelToLoad)
    {
        SceneManager.LoadScene(levelToLoad);
    }

    public void CloseLevelsWindow()
    {
        settingsWindow.SetActive(false);
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

    public void QuitGame()
    {
        Application.Quit();
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
            // If the primary button is pressed, don’t check the other controllers and return a value.
            if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton) && primaryButton)
            {
                // As long as the button has not been released, return false.
                if (menuButtonIsPressed){
                    return false;
                }
                else // If it’s the first time the button is pressed or it has been released, return true.
                {
                    menuButtonIsPressed = true;
                    return true;
                }
            }
        }

        // If the primary button is not pressed on any of the left hand devices, return false.
        menuButtonIsPressed = false;
        return false;
    }
}
