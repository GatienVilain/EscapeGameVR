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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rightTeleportation.SetActive(rightCancel.action.ReadValue<float>() ==0 && rightActivate.action.ReadValue<float>() > 0.1f);   
    }
}
