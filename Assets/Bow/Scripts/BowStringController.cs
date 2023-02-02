using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BowStringController : MonoBehaviour
{
    [SerializeField] private BowString bowStringRenderer;
    private XRGrabInteractable interactable;
    [SerializeField] private Transform midPointGrabObject, midPointVisualObject, midPointParent;
    private Transform interactor;

    [SerializeField] private float bowStringStretchLimit = 2f;

    private float strength, previousStrength;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float stringSoundThreshold = 0.001f;

    public UnityEvent OnBowPulled;
    public UnityEvent<float> OnBowReleased;

    [SerializeField] private Transform attachTransform;

    

    private void Awake()
    {
        interactable = midPointGrabObject.GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        interactable.selectEntered.AddListener(PrepareBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void ResetBowString(SelectExitEventArgs arg0)
    {
        OnBowReleased?.Invoke(strength);
        strength = 0;
        previousStrength = 0;
        audioSource.pitch = -1;
        audioSource.Stop();

        interactor = null;
        midPointGrabObject.localPosition = Vector3.zero;
        midPointVisualObject.localPosition = Vector3.zero;
        bowStringRenderer.CreateString(null);
    }

    private void PrepareBowString(SelectEnterEventArgs arg0)
    {
        interactor = arg0.interactableObject.transform;
        OnBowPulled?.Invoke();  
    }

    // Update is called once per frame
    private void Update()
    { 
        if(interactor != null)
        {
            //convert bow string mid point position to the local space of the MidPoint
            Vector3 midPointLocalSpace =
                midPointParent.InverseTransformPoint(midPointGrabObject.position); // localPosition

            //get the offset
            float midPointLocalZAbs = Mathf.Abs(midPointLocalSpace.z);

            previousStrength = strength;

            HandleStringPushedBackToStart(midPointLocalSpace);

            HandleStringPulledBackTolimit(midPointLocalZAbs, midPointLocalSpace);

            HandlePullingString(midPointLocalZAbs, midPointLocalSpace);

            bowStringRenderer.CreateString(midPointVisualObject.position);
        }

    }

    private void HandlePullingString(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        //what happens when we are between point 0 and the string pull limit
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs < bowStringStretchLimit)
        {
            if(audioSource.isPlaying == false && strength <= 0.01f)
            {
                audioSource.Play();
            }
            strength = Remap(midPointLocalZAbs, 0, bowStringStretchLimit, 0, 1);
            midPointVisualObject.localPosition = new Vector3(0, 0, midPointLocalSpace.z);

            PlayStringPullingSound();
        }
    }

    private float Remap(float value, int fromMin, float fromMax, int toMin, int toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax -toMin) + toMin;
    }

    private void HandleStringPulledBackTolimit(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        //We specify max pulling limit for the string. We don't allow the string to go any farther than "bowStringStretchLimit"
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs >= bowStringStretchLimit)
        {
            audioSource.Pause();
            strength = 1;
            //Vector3 direction = midPointParent.TransformDirection(new Vector3(0, 0, midPointLocalSpace.z));
            midPointVisualObject.localPosition = new Vector3(0, 0, -bowStringStretchLimit);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.z >= 0)
        {
            audioSource.pitch = 1;
            audioSource.Stop();
            strength = 0;
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }

    private void PlayStringPullingSound()
    {
        if(Mathf.Abs(strength - previousStrength) > stringSoundThreshold)
        {
            if(strength < previousStrength)
            {
                audioSource.pitch = -1;
            }
            else
            {
                audioSource.pitch = 1;
            }
            audioSource.UnPause();
        }
        else
        {
            audioSource.Pause();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hand"))
        {
            //Check which hand is trying to grab the bow to set correctly the
            //attach point so that the hand is correctly positionned
            if (other.name.Contains("Left"))
            {
                attachTransform.localPosition = new Vector3(0.1F, 0.15F, 0.2F);
            }
            else if (other.name.Contains("Right"))
            {
                attachTransform.localPosition = new Vector3(0.1F, -0.15F, 0.2F);
            }
        }
    }
}