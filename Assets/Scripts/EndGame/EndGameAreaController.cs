using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameAreaController : MonoBehaviour
{
    [SerializeField] private TimerHandler timerHandler;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log("EndGame");
            timerHandler.PauseTimer();
            timerHandler.SaveTime();
            SceneManager.LoadScene("EndMenu");
        }
    }
}