using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DigicodeHandler : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private TextMeshPro display;
    private string currentCode;

    // Start is called before the first frame update
    void Start()
    {
        currentCode = "";
    }

    private void UpdateDisplay()
    {

    }

    public void PressButton(int number)
    {
        currentCode += number.ToString();
        if (currentCode.Length == correctCode.Length)
        {
            if (currentCode == correctCode)
            {
                //Code is correct
            }
            else
            {
                //BEEEP code incorrect
            }
            currentCode = "";
        }
    }
}
