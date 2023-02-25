using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DigicodeHandler : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    private string currentCode = "";

    [SerializeField] private GameObject displayText;
    private TextMeshProUGUI displayTextMechPro;

    [SerializeField] private AudioSource audioCorrectCode;
    [SerializeField] private AudioSource audioWrongCode;
    
    
    // Start is called before the first frame update
    void Start()
    {
        displayTextMechPro = displayText.GetComponent<TextMeshProUGUI>();
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        displayTextMechPro.text = currentCode;
    }

    public void PressButton(int number)
    {
        currentCode += number.ToString();
        UpdateDisplay();
        if (currentCode.Length == correctCode.Length)
        {
            StartCoroutine(CheckCurrentCode());
        }
    }

    private IEnumerator CheckCurrentCode()
    {
        if (currentCode == correctCode)
        {
            displayTextMechPro.color = Color.green;
            audioCorrectCode.Play();
            //Code is correct
        }
        else
        {
            displayTextMechPro.color = Color.red;
            audioWrongCode.Play();
            //BEEEP code incorrect
        }
        yield return new WaitForSeconds(1);
        currentCode = "";
        UpdateDisplay();
        displayTextMechPro.color = Color.white;
    }
}
