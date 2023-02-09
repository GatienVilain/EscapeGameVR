using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

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
        if (Input.GetButtonDown("Cancel"))
        {
            if (inSettingsWindow)
            {
                CloseSettingsWindow();
            }
        }
    }

    IEnumerator SetMenu()
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
}
