using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformControllerOpenExit : MonoBehaviour
{
    [SerializeField] private Animator exitAnim;
    [SerializeField] private AudioSource exitSound;
    private bool exitOpen;

    private void Awake()
    {
        exitOpen = false;
    }

    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels à la méthode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (!exitOpen && collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial)

            {
                Debug.Log("Open Exit");
                exitAnim.SetTrigger("TriggerOpenExit");
                exitOpen = true;
                StartCoroutine(PlaySound());
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (exitOpen && collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial)

            {
                exitOpen = false;
                Debug.Log("Close Exit");
                exitAnim.SetTrigger("TriggerCloseExit");
                StartCoroutine(PlaySound());
            }
        }
    }

    private IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(0.5f);
        exitSound.Play();
    }
}
