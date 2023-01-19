using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlatformController : MonoBehaviour
{

    [SerializeField] private Material newMaterial;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels à la méthode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial)

            {
                Destroy(this.gameObject);
            }
            else
            {
                collision.gameObject.GetComponent<Renderer>().material = newMaterial;
            }
        }
    }

}
