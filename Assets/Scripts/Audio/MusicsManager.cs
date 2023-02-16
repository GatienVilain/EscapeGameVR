using UnityEngine;

public class MusicsManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource = default;
    [SerializeField] private AudioClip[] playlist = default;

    [Header("Debug Settings")]
    [Tooltip("Gives the ability to move easily to the next music")]
    [SerializeField] private bool playNextMusic = false;

    private int musicIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioSource.clip = playlist[0];
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if ( !audioSource.isPlaying )
        {
            PlayNextMusic();
        }
        // Debug feature
        else if( playNextMusic )
        {
            PlayNextMusic();
            playNextMusic = false;
        }
    }

    private void PlayNextMusic()
    {
        musicIndex = ( musicIndex + 1 )% playlist.Length;
        audioSource.clip = playlist[musicIndex];
        audioSource.Play();
    }
}
