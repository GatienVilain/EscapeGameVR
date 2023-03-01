using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowDropSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource bowDropSound;
    private bool hasBeenPlayed = false;


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "bowSound" && !hasBeenPlayed)
        {
            bowDropSound.Play();
        }
    }

}
