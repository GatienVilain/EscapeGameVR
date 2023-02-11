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

    [SerializeField] private Transform rightHand;
    [SerializeField] private Transform leftHand;


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
                rightHand.localScale -= speed * Time.deltaTime * new Vector3(1, 1, 1);
                leftHand.localScale -= speed * Time.deltaTime * new Vector3(1, 1, 1);
                if (transform.position.y <= downPositionY)
                {
                    transform.position = new Vector3(transform.position.x, downPositionY, transform.position.z);
                    versbas = false;
                    bas = true;
                    
 
                }

                if (rightHand.localScale.x <= 0.5f)
                {
                    rightHand.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    leftHand.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }

                if (rightHand.localScale.x <= 0.5f && transform.position.y <= downPositionY)
                {
                    vershaut = false;
                }

            }
            if (vershaut)
            {
                transform.position += new Vector3(0f, speed*Time.deltaTime , 0f);
                rightHand.localScale += new Vector3(speed*Time.deltaTime, speed * Time.deltaTime, speed * Time.deltaTime);
                leftHand.localScale += new Vector3(speed * Time.deltaTime, speed * Time.deltaTime, speed * Time.deltaTime);
                bas = false;
                if (transform.position.y >= originalPositionY)
                {
                    transform.position = new Vector3(transform.position.x, originalPositionY, transform.position.z);                
                }

                if(rightHand.localScale.x >= 1f)
                {
                    rightHand.localScale = new Vector3(1f, 1f, 1f);
                    leftHand.localScale = new Vector3(1f, 1f, 1f);
                }

                if(rightHand.localScale.x >= 1f && transform.position.y >= originalPositionY)
                {
                    vershaut = false;
                }
            }
        }



            
        
    }
}
