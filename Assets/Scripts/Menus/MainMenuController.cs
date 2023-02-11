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

    private static bool inSettingsWindow = false;

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
