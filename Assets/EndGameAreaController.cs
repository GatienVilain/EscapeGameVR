using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameAreaController : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("EndGame");
        }

    }
}
