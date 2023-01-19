using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseWindow = default;
    [SerializeField] private GameObject pauseOptionWindow = default;
    [SerializeField] private GameObject settingsWindow = default;

    public static bool gameIsPaused = false;

    private static bool inSettingsWindow = false;

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
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
        pauseOptionWindow.SetActive(false);
        inSettingsWindow = true;
    }

    // Exit the setting window.
    public void CloseSettingsWindow()
    {
        pauseOptionWindow.SetActive(true);
        settingsWindow.SetActive(false);
        inSettingsWindow = false;
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
        pauseWindow.SetActive(true);
        Time.timeScale = 0;
        gameIsPaused = true;
    }
}
