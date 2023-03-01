using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerHandler : MonoBehaviour
{
    private float currentTime = 0;
    private int currentSeconds = 0;
    private int heures = 0;
    private int minutes = 0;
    private int seconds = 0;
    private bool isRunning = true;
    [SerializeField] private TextMeshProUGUI display;
    [SerializeField] private SavedTime timeSaved;

    // Update is called once per frame
    private void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            currentSeconds = Mathf.FloorToInt(currentTime % 60);
            if (currentSeconds != seconds)  //Si une seconde s'est �coul� depuis la derni�re mise � jour du texte, mettre � jour le timer
            {
                seconds = currentSeconds;
                heures = Mathf.FloorToInt(currentSeconds / 3600);
                minutes = Mathf.FloorToInt((currentTime % 3600) / 60);
                display.text = $"{heures:d2}:{minutes:d2}:{seconds:d2}";
            }
        }
    }

    public void PauseTimer()
    {
        isRunning = false;  
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        currentTime = 0;
        isRunning = true;
    }

    public void SaveTime()
    {
        timeSaved.time = currentTime;
    }

    public string GetTime()
    {
        return display.text;
    }
}
