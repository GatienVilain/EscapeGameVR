using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAreaController : MonoBehaviour
{
    public static bool isInExitArea = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isInExitArea && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log("Collision Head");
            isInExitArea = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (isInExitArea && other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isInExitArea = false;

        }
    }
}
