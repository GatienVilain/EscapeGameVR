using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeIsSitting : MonoBehaviour
{
    [SerializeField] private isSittingScriptableObject isSittingSettings;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        if (isSittingSettings.isSitting)
        {
            text.text = "Cliquez ici pour jouer debout";
        }
        else
        {
            text.text = "Cliquez ici pour jouer assis";
        }
    }

    public void ChangeIsSittingMethod()
    {
        if (isSittingSettings.isSitting)
        {
            isSittingSettings.isSitting = false;
            text.text = "Cliquez ici pour jouer assis";
        }
        else
        {
            isSittingSettings.isSitting = true;
            text.text = "Cliquez ici pour jouer debout";
        }
    }
}
