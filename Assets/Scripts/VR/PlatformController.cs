using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlatformController : MonoBehaviour
{
    [SerializeField] private Vector3 arcPosition;
    [SerializeField] private GameObject arcPrefab;
    [SerializeField] private int numberOfPlatform;
    [SerializeField] private Vector3 rotation;

    private static int numberCorrectCube;
    private static bool arcDroped;
    private Quaternion arcRotation;
    // Start is called before the first frame update
    void Awake()
    {
        numberCorrectCube = 0;
        arcDroped = false;
        arcRotation = Quaternion.Euler(rotation);
    }

    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels à la méthode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial && !arcDroped)

            {
                numberCorrectCube++;
                if (numberCorrectCube == numberOfPlatform)
                {
                    Instantiate(arcPrefab, arcPosition, arcRotation);
                    arcDroped = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial && !arcDroped)

            {
                numberCorrectCube--;
            }
        }
    }

}