using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickingArrowToSurface : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private SphereCollider myCollider;
    [SerializeField] private GameObject strickingArrow;

    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(3);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.attachedRigidbody != null && collision.collider.gameObject.layer == LayerMask.NameToLayer("target"))
        {
            rb.isKinematic = true;
            myCollider.isTrigger = true;

            GameObject arrow = Instantiate(strickingArrow);
            arrow.transform.position = transform.position;
            arrow.transform.forward = transform.forward;
            arrow.transform.parent = collision.collider.attachedRigidbody.transform;

            collision.collider.GetComponent<IHittable>()?.GetHit();

            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(DestroyArrow());
        }
        

    }

}
