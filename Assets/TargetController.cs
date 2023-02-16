using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] Animator door_ANIM;
    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        hasEntered = false;
        Debug.Log("hasEntered reset");
        door_ANIM.SetTrigger("TriggerCloseDoor");
        // Code to execute after the delay
    }

    private bool hasEntered;
    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels à la méthode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (!hasEntered && collision.gameObject.layer == LayerMask.NameToLayer("Arrow"))
        {
            Debug.Log("Cible touchée");
            door_ANIM.SetTrigger("TriggerOpenDoor");
            hasEntered = true;
            StartCoroutine(ExecuteAfterTime(10));
        }
    }
}
