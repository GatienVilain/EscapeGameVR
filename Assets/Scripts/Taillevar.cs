using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taillevar : MonoBehaviour
{

    [SerializeField] private Transform cameraTransform;
    //[SerializeField] private float increment =0.6f;
    [SerializeField] private float speed =1f;
    
    private bool bas = false;
    private bool versbas = false;
    private bool vershaut = false;
    private float downPositionY;
    //private float timeleft;




    // Start is called before the first frame update
    void Start()
    {
        //transform.position = new Vector3(0f, 1f, 0f);
        downPositionY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {

        if(bas)
        {
            transform.position.Set(transform.position.x, 0.35f, transform.position.z);
        }


            if(Input.GetButtonDown("Fire1"))
        {

            if (!(bas))
            {
                versbas=true;

            }
            else
            {
                vershaut=true;
            }
        }



        if ((versbas) || (vershaut))
        {
            if (versbas)
            {
                transform.position -= new Vector3(0f, speed*Time.deltaTime , 0f);
                if (transform.position.y <= 0.35f )
                {
                    versbas = false;
                    bas=true;
                }
            }
            if (vershaut)
            {
                transform.position += new Vector3(0f, speed*Time.deltaTime , 0f);
                if (transform.position.y >= 1f)
                {
                    vershaut = false;
                    bas=false;
                }
            }
        }



            
        
    }
}
