using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyStickingArrow : MonoBehaviour
{
    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(5);

        Destroy(gameObject);
    }

    private void Awake()
    {
        StartCoroutine(DestroyArrow());
    }
}
