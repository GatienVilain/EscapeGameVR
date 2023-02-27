using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGame : MonoBehaviour
{
    [SerializeField] private GameObject timer;
    private TimerHandler timerHandler;

    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        timerHandler = timer.GetComponent<TimerHandler>();

        timerHandler.SetFinished();
        text.text = $"Félicitation, vous avez fini le jeu en {timerHandler.GetMinutes()} minutes et {timerHandler.GetSeconds()} secondes !";
    }
}
