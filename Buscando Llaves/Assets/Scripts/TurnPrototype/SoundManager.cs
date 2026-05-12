using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;
    [SerializeField] private bool loopMusic = true;

    [Header("Global Volume")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool muteMusic;
    [SerializeField] private bool muteSfx;

    [Header("SFX - Movement")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip movementDeniedClip;
    [SerializeField] private bool randomizeFootstepPitch = true;
    [Range(0.7f, 1.3f)] [SerializeField] private float footstepPitchMin = 0.92f;
    [Range(0.7f, 1.3f)] [SerializeField] private float footstepPitchMax = 1.08f;

    [Header("SFX - Cards")]
    [SerializeField] private AudioClip numericCardClip;
    [SerializeField] private AudioClip tristezaCardClip;
    [SerializeField] private AudioClip retornoCardClip;
    [SerializeField] private AudioClip stopCardClip;

    [Header("SFX - Gameplay")]
    [SerializeField] private AudioClip diceRollClip;
    [SerializeField] private AudioClip loseLifeClip;
    [SerializeField] private AudioClip chestFailClip;
    [SerializeField] private AudioClip chestOpenClip;
    [SerializeField] private AudioClip keyFoundClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip defeatClip;

    [Header("Behavior")]
    [SerializeField] private bool persistAcrossScenes = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        EnsureSources();
        ApplyVolumes();
    }

    private void Start()
    {
        if (playMusicOnStart)
        {
            PlayBackgroundMusic();
        }
    }

    private void OnValidate()
    {
        if (musicSource == null || sfxSource == null)
        {
            return;
        }

        ApplyVolumes();
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null || backgroundMusic == null)
        {
            return;
        }

        musicSource.clip = backgroundMusic;
        musicSource.loop = loopMusic;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.Stop();
    }

    public void SetMusicPaused(bool paused)
    {
        if (musicSource == null)
        {
            return;
        }

        if (paused)
        {
            musicSource.Pause();
            return;
        }

        musicSource.UnPause();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetMusicMute(bool value)
    {
        muteMusic = value;
        ApplyVolumes();
    }

    public void SetSfxMute(bool value)
    {
        muteSfx = value;
        ApplyVolumes();
    }

    public void PlayFootstep()
    {
        if (footstepClip == null || sfxSource == null || muteSfx)
        {
            return;
        }

        float originalPitch = sfxSource.pitch;
        if (randomizeFootstepPitch)
        {
            sfxSource.pitch = UnityEngine.Random.Range(footstepPitchMin, footstepPitchMax);
        }

        sfxSource.PlayOneShot(footstepClip, masterVolume * sfxVolume);
        sfxSource.pitch = originalPitch;
    }

    public void PlayMoveDenied()
    {
        PlaySfx(movementDeniedClip);
    }

    public void PlayNumericCard()
    {
        PlaySfx(numericCardClip);
    }

    public void PlayTristezaCard()
    {
        PlaySfx(tristezaCardClip);
    }

    public void PlayRetornoCard()
    {
        PlaySfx(retornoCardClip);
    }

    public void PlayStopCard()
    {
        PlaySfx(stopCardClip);
    }

    public void PlayLoseLife()
    {
        PlaySfx(loseLifeClip);
    }

    public void PlayDiceRoll()
    {
        PlaySfx(diceRollClip);
    }

    public void PlayChestOpenFail()
    {
        PlaySfx(chestFailClip);
    }

    public void PlayChestOpen()
    {
        PlaySfx(chestOpenClip);
    }

    public void PlayKeyFound()
    {
        PlaySfx(keyFoundClip);
    }

    public void PlayVictory()
    {
        PlaySfx(victoryClip);
    }

    public void PlayDefeat()
    {
        PlaySfx(defeatClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null || muteSfx)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume);
    }

    private void EnsureSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = loopMusic;
        musicSource.spatialBlend = 0f;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = muteMusic ? 0f : masterVolume * musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = muteSfx ? 0f : masterVolume * sfxVolume;
        }
    }
}
