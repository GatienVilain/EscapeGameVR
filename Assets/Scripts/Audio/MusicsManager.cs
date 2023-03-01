using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicsManager : MonoBehaviour
{
    [Tooltip("Time in seconds to transition between two musics")]
    [SerializeField] private float transitionTime = 5f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip mainMenuMusic = default;
    [SerializeField] private AudioClip endMenuMusic = default;
    [SerializeField] private AudioClip[] gamePlaylist = default;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource1 = default;
    [SerializeField] private AudioSource audioSource2 = default;

    [Header("Debug Settings")]
    [Tooltip("Play the next music in the playlist")]
    [SerializeField] private bool playNextMusic = false;

    private string scene =  "";
    private bool stopTransition = false;
    private bool stopWaitForEnd = false;
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

    // S’il y a pas de musique en cours, que la scene à changé ou que l’on veut passer à la musique suivante (débug), on joue la musique suivante
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

        if(currentScene == "SampleScene" || currentScene == "Salle tutoriel")
        {
            // On joue la musique suivante dans la playlist
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
    }

    private IEnumerator SoftTransition(AudioClip newMusic)
    {
        // On récupère la source audio qui joue actuellement et la source audio qui va jouer la nouvelle musique
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

        // On donne la nouvelle musique à la source audio qui va la jouer
        newTrack.clip = newMusic;
        newTrack.Play();

        // S’il y a une transition en cours et que la scene change, on fait en sorte que la nouvelle coroutine puisse continuer de se jouer.
        // Explication:
        //      S’il y a une transtion en cours et que la scene change, on se retrouve avec deux coroutines qui fonctionnent en même temps.
        //      Il faut donc arrêter la première et laisser la seconde continuer.
        //      Pour cela, on met stopTransition à true juste avant de lancer la nouvelle coroutine (voir Update).
        //  	Et comme la première coroutine se trouve dans la boucle while, elle va arrêter la transition en cours. (voir la boucle while ci-dessous)
        //  	Enfin, on met stopTransition à false pour que la nouvelle coroutine puisse continuer de se jouer.
        stopTransition = false;

        // On fait une transition entre les deux musiques
        float percentage = 0;
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
                // Si la scene change alors qu'on est dans cette boucle, alors on arrête la coroutine
                stopTransition = false;
                yield break;
            }
        }
        fadingTrack.Stop();

        float timeToWait = newMusic.length - 2 * newTrack.time - 0.5f;
        StartCoroutine(WaitForEnd(timeToWait));
    }

    // On attend la fin de la musique pour relancer la prochaine musique
    private IEnumerator WaitForEnd(float timeToWait)
    {
        float time = 0;
        while (time < timeToWait)
        {
            if (stopWaitForEnd)
            {
                // Si la scene change, alors on ne relance pas la prochaine musique ici
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
}
