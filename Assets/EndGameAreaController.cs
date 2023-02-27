using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class EndGameAreaController : MonoBehaviour
{
    [SerializeField] GameObject prefabEndGame;
    [SerializeField] private GameObject mainCamera = default;
    [SerializeField] private GameObject endGameWindow = default;
    [SerializeField] private GameObject canvas = default;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log("EndGame");
            canvas.GetComponent<PauseMenuController>().SetMenuPosition();
            endGameWindow.SetActive(true);
            Time.timeScale = 0;

        }

    }
}
