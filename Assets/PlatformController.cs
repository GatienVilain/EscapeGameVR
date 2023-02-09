using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlatformController : MonoBehaviour
{

    [SerializeField] private Material newMaterial;
    [SerializeField] private Vector3 arcPosition;
    [SerializeField] private GameObject arcPrefab;
    [SerializeField] private int numberOfPlatform;
    [SerializeField] private Vector3 rotation;

    private static int numberCorrectCube;
    private static bool isArcDroped;
    private Quaternion arcRotation;
    // Start is called before the first frame update
    void Start()
    {
        numberCorrectCube = 0;
        isArcDroped = false;
        arcRotation = Quaternion.Euler(rotation);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision) //Un appel par collision, donc si l'objet touche 6 autres objets, 6 appels à la méthode, onCollisionEnter2D fonctionne avec des rigidbody2D
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial && !isArcDroped)

            {
                numberCorrectCube++;
                Debug.Log("Add 1 correct cube");
                if (numberCorrectCube == numberOfPlatform)
                {
                    Instantiate(arcPrefab, arcPosition, arcRotation);
                    isArcDroped = true;
                    Debug.Log("Drop arc");
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("ColoredCube"))
        {
            if (collision.gameObject.GetComponent<Renderer>().sharedMaterial == this.gameObject.GetComponent<Renderer>().sharedMaterial && !isArcDroped)

            {
                numberCorrectCube--;
                Debug.Log("Remove 1 correct cube");
            }
        }
    }

}