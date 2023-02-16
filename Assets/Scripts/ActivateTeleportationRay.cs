using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ActivateTeleportationRay : MonoBehaviour
{
    [SerializeField] GameObject rightTeleportation;

    [SerializeField] InputActionProperty rightActivate;

    [SerializeField] InputActionProperty rightCancel;

    [SerializeField] GameObject leftTeleportation;

    [SerializeField] InputActionProperty leftActivate;

    [SerializeField] InputActionProperty leftCancel;

    [SerializeField] GameObject overlay;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!PauseMenuController.gameIsPaused)
        {
            rightTeleportation.SetActive(rightCancel.action.ReadValue<float>() == 0 && rightActivate.action.ReadValue<float>() > 0.1f && !overlay.GetComponent<VRNoPeeking>().IsCameraFadedOut());
            leftTeleportation.SetActive(leftCancel.action.ReadValue<float>() == 0 && leftActivate.action.ReadValue<float>() > 0.1f && !overlay.GetComponent<VRNoPeeking>().IsCameraFadedOut());
        }
    }
}
