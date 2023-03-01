using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAttachPointSwitcher : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hand"))
        {
            Vector3 attachTransform = transform.localPosition;

            //Check which hand is trying to grab the bow to set correctly the
            //attach point so that the hand is correctly positionned
            if (other.name.Contains("Left"))
            {
                if(attachTransform.y < 0)
                {
                    attachTransform.y = -attachTransform.y;
                }
            }
            else if (other.name.Contains("Right"))
            {
                if (attachTransform.y > 0)
                {
                    attachTransform.y = -attachTransform.y;
                }
            }
            transform.localPosition = attachTransform;
        }
    }
}
