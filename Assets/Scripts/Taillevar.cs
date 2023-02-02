using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taillevar : MonoBehaviour
{

    [SerializeField] private Transform cameraTransform;
    //[SerializeField] private float increment =0.6f;
    [SerializeField] private float speed =1f;
    
    private bool bas;
    private bool versbas;
    private bool vershaut;
    private float downPositionY;
    private float originalPositionY;

    [SerializeField] private float sizeDecrementValue = 0.35f;
    //private float timeleft;




    // Start is called before the first frame update
    void Start()
    {
        //transform.position = new Vector3(0f, 1f, 0f);
        bas = false;
        versbas = false;
        vershaut = false;
        originalPositionY = transform.position.y ;
        downPositionY = transform.position.y - sizeDecrementValue;
    }

    // Update is called once per frame
    void Update()
    {

        if(bas)
        {
            transform.position = new Vector3(transform.position.x, downPositionY, transform.position.z);
            Debug.Log("test");
        }


        if(Input.GetButtonDown("Fire1"))
        {

            if (!(bas))
            {
                versbas=true;
            }
            else
            {
                bas = false;
                vershaut=true;
            }
        }



        if ((versbas) || (vershaut))
        {
            if (versbas)
            {
                transform.position -= new Vector3(0f, speed*Time.deltaTime , 0f);
                if (transform.position.y <= downPositionY)
                {
                    transform.position = new Vector3(0f, downPositionY, 0f);
                    versbas = false;
                    bas=true;
                }
            }
            if (vershaut)
            {
                transform.position += new Vector3(0f, speed*Time.deltaTime , 0f);
                if (transform.position.y >= originalPositionY)
                {
                    transform.position = new Vector3(0f, originalPositionY, 0f);
                    vershaut = false;
                    bas=false;
                }
            }
        }



            
        
    }
}
