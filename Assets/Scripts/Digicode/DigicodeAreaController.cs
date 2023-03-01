using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigicodeAreaController : MonoBehaviour
{

    [SerializeField] GameObject rightTeleportationRay;
    [SerializeField] GameObject leftTeleportationRay;
    [SerializeField] GameObject rightRayInteractor;
    [SerializeField] GameObject leftRayInteractor;

    public static bool isInDigicodeArea = false;

    // Start is called before the first frame update
    void Start()
    {
        isInDigicodeArea = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInDigicodeArea && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log("Collision Head");
            isInDigicodeArea = true;
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        if (isInDigicodeArea && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isInDigicodeArea = false;

        }
    }

}
