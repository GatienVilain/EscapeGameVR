using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;

    [SerializeField] private SavedTime savedTime;
    private float time;

    [SerializeField] private GameObject textObject;
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = textObject.GetComponent<TextMeshProUGUI>();
        time = savedTime.time;

        text.text = $"Vous vous êtes échappés !\n\nTemps : \n{Mathf.FloorToInt(time / 3600)}h {Mathf.FloorToInt((time % 3600) / 60)}min {Mathf.FloorToInt(time % 60)}sec";
    }

    public void MoveToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
