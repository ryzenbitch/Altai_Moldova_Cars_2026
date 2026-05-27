using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusic;
    public AudioClip galleryMusic;

    [Header("SFX")]
    public AudioClip cardDealClip;
    public AudioClip cardFlipClip;
    public AudioClip cardSlideClip;
    public AudioClip buttonClickClip;
    public AudioClip winRoundClip;
    public AudioClip loseRoundClip;
    public AudioClip gameWinClip;
    public AudioClip gameLoseClip;
    public AudioClip eliminatedClip;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // Create AudioSources if not assigned
        if (!musicSource)
        {
            musicSource       = gameObject.AddComponent<AudioSource>();
            musicSource.loop  = true;
            musicSource.playOnAwake = false;
        }
        if (!sfxSource)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        if (musicSource) musicSource.volume = GameSettings.MusicVolume;
        // sfxSource volume set per PlayOneShot call
    }

    // ── Music ─────────────────────────────────────────────────────────────
    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
    public void PlayGameMusic()     => PlayMusic(gameMusic);
    public void PlayGalleryMusic()  => PlayMusic(galleryMusic);

    void PlayMusic(AudioClip clip)
    {
        if (!musicSource || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = GameSettings.MusicVolume;
        musicSource.Play();
    }

    // ── SFX ───────────────────────────────────────────────────────────────
    public void PlayCardDeal()    => PlaySFX(cardDealClip);
    public void PlayCardFlip()    => PlaySFX(cardFlipClip);
    public void PlayCardSlide()   => PlaySFX(cardSlideClip);
    public void PlayButtonClick() => PlaySFX(buttonClickClip);
    public void PlayWinRound()    => PlaySFX(winRoundClip);
    public void PlayLoseRound()   => PlaySFX(loseRoundClip);
    public void PlayGameWin()     => PlaySFX(gameWinClip);
    public void PlayGameLose()    => PlaySFX(gameLoseClip);
    public void PlayEliminated()  => PlaySFX(eliminatedClip);

    void PlaySFX(AudioClip clip)
    {
        if (!sfxSource || clip == null) return;
        sfxSource.PlayOneShot(clip, GameSettings.SFXVolume);
    }
}
