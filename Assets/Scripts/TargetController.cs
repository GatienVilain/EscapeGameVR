using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [SerializeField] private Animator door_ANIM;
    [SerializeField] private AudioSource doorOpenSound;
    private static int numberOfTargetLeft;
    private bool hasEntered;

    private void Awake()
    {
        numberOfTargetLeft = 2;
        hasEntered = false;
    }
    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels � la m�thode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (!hasEntered && collision.gameObject.layer == LayerMask.NameToLayer("Arrow"))
        {
            numberOfTargetLeft--;
            if(numberOfTargetLeft == 0)
            {
                door_ANIM.SetTrigger("TriggerOpenDoor");
                doorOpenSound.Play();
            }
            
            hasEntered = true;
            
        }
    }
}
