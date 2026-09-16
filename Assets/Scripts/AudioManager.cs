using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip startSceneMusic;
    [SerializeField] private AudioClip ghostsNormalMusic;
    [SerializeField] private AudioClip ghostsScaredMusic;
    [SerializeField] private AudioClip ghostDeadMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip pacStudentMoveSFX;
    [SerializeField] private AudioClip eatPelletSFX;
    [SerializeField] private AudioClip eatGhostSFX;
    [SerializeField] private AudioClip eatCherrySFX;
    [SerializeField] private AudioClip wallCollideSFX;
    [SerializeField] private AudioClip deathSFX;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource movementSource;

    private Coroutine introTransitionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (movementSource == null)
        {
            movementSource = gameObject.AddComponent<AudioSource>();
            movementSource.playOnAwake = false;
            movementSource.loop = true;
        }
    }

    private void Start()
    {
        PlayIntroSequence();
    }

    public void PlayIntroSequence()
    {
        if (introTransitionCoroutine != null)
        {
            StopCoroutine(introTransitionCoroutine);
        }
        introTransitionCoroutine = StartCoroutine(IntroSequenceRoutine());
    }

    private IEnumerator IntroSequenceRoutine()
    {
        if (introMusic != null)
        {
            musicSource.clip = introMusic;
            musicSource.loop = false;
            musicSource.Play();

            float timer = 0f;
            float maxWait = Mathf.Min(3.0f, introMusic.length);

            while (timer < maxWait && musicSource.isPlaying)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(3.0f);
        }

        // Switch to ghosts normal state on loop
        PlayGhostsNormalMusic();
    }

    public void PlayGhostsNormalMusic()
    {
        if (ghostsNormalMusic != null)
        {
            musicSource.clip = ghostsNormalMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayGhostsScaredMusic()
    {
        if (ghostsScaredMusic != null)
        {
            musicSource.clip = ghostsScaredMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayGhostDeadMusic()
    {
        if (ghostDeadMusic != null)
        {
            musicSource.clip = ghostDeadMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayMovementSFX(bool play)
    {
        if (movementSource == null) return;

        if (play)
        {
            if (pacStudentMoveSFX != null && (!movementSource.isPlaying || movementSource.clip != pacStudentMoveSFX))
            {
                movementSource.clip = pacStudentMoveSFX;
                movementSource.loop = true;
                movementSource.Play();
            }
        }
        else
        {
            if (movementSource.isPlaying)
            {
                movementSource.Stop();
            }
        }
    }

    public void PlayEatPelletSFX()
    {
        if (sfxSource != null && eatPelletSFX != null)
        {
            sfxSource.PlayOneShot(eatPelletSFX);
        }
    }

    public void PlayEatGhostSFX()
    {
        if (sfxSource != null && eatGhostSFX != null)
        {
            sfxSource.PlayOneShot(eatGhostSFX);
        }
    }

    public void PlayEatCherrySFX()
    {
        if (sfxSource != null && eatCherrySFX != null)
        {
            sfxSource.PlayOneShot(eatCherrySFX);
        }
    }

    public void PlayWallCollideSFX()
    {
        if (sfxSource != null && wallCollideSFX != null)
        {
            sfxSource.PlayOneShot(wallCollideSFX);
        }
    }

    public void PlayDeathSFX()
    {
        if (sfxSource != null && deathSFX != null)
        {
            sfxSource.PlayOneShot(deathSFX);
        }
    }
}
