using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerHandler : MonoBehaviour
{
    private float currentTime = 0;
    private int currentSeconds = 0;
    private int minutes = 0;
    private int seconds = 0;
    private bool isFinished = false;
    private TextMeshProUGUI display;


    // Start is called before the first frame update
    private void Start()
    {
        display = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isFinished)
        {
            currentTime += Time.deltaTime;
            currentSeconds = Mathf.FloorToInt(currentTime % 60);
            if (currentSeconds != seconds)  //Si une seconde s'est écoulé depuis la dernière mise à jour du texte, mettre à jour le timer
            {
                seconds = currentSeconds;
                minutes = Mathf.FloorToInt(currentTime / 60);
                display.text = minutes + ":" + seconds;
            }
        }
    }

    public void SetFinished()
    {
        isFinished = true;  
    }

    public int GetMinutes()
    {
        return minutes;
    }

    public int GetSeconds()
    {
        return seconds;
    }

}
