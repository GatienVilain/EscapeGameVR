using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject levelWindow;
    [SerializeField] private GameObject settingsWindow;

    void Start()
    {
        Vector3 position = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
        transform.position = mainCamera.transform.position + position;
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
    }

    // Exit the setting window.
    public void CloseSettingsWindow()
    {
        settingsWindow.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
