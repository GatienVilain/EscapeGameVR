using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicsManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource1 = default;
    [SerializeField] private AudioSource audioSource2 = default;

    [SerializeField] private AudioClip[] gamePlaylist = default;
    [SerializeField] private AudioClip mainMenuMusic = default;
    [SerializeField] private AudioClip endMenuMusic = default;

    private float transitionTime = 5f;

    private string scene =  "";
    private bool stopTransition = false;
    private bool stopWaitForEnd = false;

    [Header("Debug Settings")]
    [Tooltip("Gives the ability to move easily to the next music")]
    [SerializeField] private bool playNextMusic = false;

    private int musicIndex = -1;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("music");

        if (objs.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if ( !audioSource1.isPlaying && !audioSource2.isPlaying )
        {
            //Au début du jeu ou si la musique s'arrête pour une raison ou pour une autre (cela ne devrait pas arriver)
            PlayNextMusic();
            scene = currentScene;
        }
        else if (scene != currentScene)
        {
            //Si on change de scene
            stopTransition= true;
            stopWaitForEnd = true;
            PlayNextMusic();
            scene = currentScene;
        }
        // Debug feature
        else if( playNextMusic )
        {
            //Si on veut passer à la prochaine musique
            stopTransition = true;
            stopWaitForEnd = true;
            PlayNextMusic();
            playNextMusic = false;
        }
    }

    private void PlayNextMusic()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if(currentScene == "SampleScene")
        {
            musicIndex = (musicIndex + 1) % gamePlaylist.Length;
            AudioClip newMusic = gamePlaylist[musicIndex];
            StartCoroutine(SoftTransition(newMusic));
        }
        else if(currentScene == "MainMenu")
        {
            StartCoroutine(SoftTransition(mainMenuMusic));
        }
        else if(currentScene == "EndMenu")
        {
            StartCoroutine(SoftTransition(endMenuMusic));
        }
        else
        {
            //debug***************************
            Debug.Log("error scene");
        }
    }

    private IEnumerator WaitForEnd(float timeToWait)
    {
        float time = 0;
        while(time < timeToWait)
        {
            if (stopWaitForEnd)
            {
                //Si la scene change, alors on ne relance pas la prochaine musique ici
                stopWaitForEnd = false;
                yield break;
            }
            else
            {
                time += Time.deltaTime;
                yield return null;
            }
        }
        
        PlayNextMusic();
    }

    private IEnumerator SoftTransition(AudioClip newMusic)
    {
        AudioSource fadingTrack;
        AudioSource newTrack;
        if (audioSource1.isPlaying)
        {
            fadingTrack = audioSource1;
            newTrack = audioSource2;
        }
        else
        {
            fadingTrack = audioSource2;
            newTrack = audioSource1;
        }

        float percentage = 0;
        newTrack.clip = newMusic;

        //Si on relance la musique parce que la scene a changé, on n'arrête pas cette transition
        stopTransition = false; 
        newTrack.Play();

        while (fadingTrack.volume > 0)
        {
            if (!stopTransition)
            {
                fadingTrack.volume = Mathf.Lerp(1, 0, percentage);
                newTrack.volume = Mathf.Lerp(0, 1, percentage);
                percentage += Time.deltaTime / transitionTime;
                yield return null;
            }
            else
            {
                //Si la scene change alors qu'on est dans cette boucle, alors on arrête la coroutine
                stopTransition = false;
                yield break;
            }
        }

        fadingTrack.Stop();

        float timeToWait = newMusic.length - 2 * newTrack.time - 0.5f;

        StartCoroutine(WaitForEnd(timeToWait));

    }


}
