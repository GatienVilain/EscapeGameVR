using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;

    private TimerHandler timerHandler;

    [SerializeField] private GameObject textObject;
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(SetEndMenu());

        text = textObject.GetComponent<TextMeshProUGUI>();
        timerHandler = GameObject.Find("Timer").GetComponent<TimerHandler>();

        timerHandler.PauseTimer();

        text.text = $"Félicitation, vous avez fini le jeu en {timerHandler.GetMinutes()} minutes et {timerHandler.GetSeconds()} secondes !";
    }

    // Set the menu in front of the camera and rotate it to face the camera.
    private IEnumerator SetEndMenu()
    {
        yield return new WaitForSeconds(1);
        Vector3 position = new Vector3(mainCamera.transform.forward.x, -0.1f, mainCamera.transform.forward.z);
        transform.position = mainCamera.transform.position + 2 * position;

        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void MoveToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
