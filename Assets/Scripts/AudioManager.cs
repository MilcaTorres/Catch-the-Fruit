using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Reproduce la música y los efectos de sonido de todo el juego.
///
/// Singleton persistente: la primera instancia que despierta se marca con DontDestroyOnLoad
/// y sobrevive a los cambios de escena (la música no se corta). Cualquier otra copia que
/// venga en una escena (p. ej. al volver al menú) se destruye en su Awake.
///
/// No hace falta llamarlo desde la lógica del juego: en cada carga de escena se suscribe
/// a los eventos del GameManager (si hay uno) y engancha el clic a todos los botones.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip catchFruit;
    [SerializeField] private AudioClip bomb;
    [SerializeField] private AudioClip loseLife;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip newRecord;
    [SerializeField] private AudioClip buttonClick;

    [Header("Volumen")]
    [Tooltip("Volumen de la música de fondo.")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

    [Tooltip("Volumen de los efectos de sonido.")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

    [Header("Pausa")]
    [Tooltip("Si es verdadero la música se pausa; si es falso solo baja de volumen.")]
    [SerializeField] private bool pauseMusicWhenPaused = false;

    [Tooltip("Fracción del volumen de música mientras el juego está en pausa (si no se pausa).")]
    [Range(0f, 1f)] [SerializeField] private float pausedMusicFactor = 0.3f;

    [Tooltip("Segundos que tarda la música en bajar/subir de volumen.")]
    [Min(0f)] [SerializeField] private float musicFadeTime = 0.25f;

    [Header("Récord")]
    [Tooltip("Segundos tras el sonido de Game Over antes de sonar el de nuevo récord.")]
    [Min(0f)] [SerializeField] private float newRecordDelay = 0.8f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private GameManager gm;
    private bool musicDucked;
    private Coroutine newRecordRoutine;

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp01(value); if (sfxSource != null) sfxSource.volume = sfxVolume; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Ya hay un AudioManager vivo (venía de una escena anterior): esta copia sobra.
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Solo funciona con objetos raíz.

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (backgroundMusic != null) musicSource.Play();
        // sceneLoaded de la primera escena se dispara antes de que exista este objeto
        // cuando se entra en Play directamente, así que se enlaza a mano.
        BindScene();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindGameManager();
        Instance = null;
    }

    private void Update()
    {
        // AudioSource no depende de Time.timeScale; el fundido usa tiempo real para
        // funcionar también con el juego en pausa (timeScale = 0).
        float target = musicVolume * (musicDucked ? pausedMusicFactor : 1f);
        float step = musicFadeTime > 0f ? Time.unscaledDeltaTime / musicFadeTime : 1f;
        musicSource.volume = Mathf.MoveTowards(musicSource.volume, target, step);
    }

    private void OnValidate()
    {
        // Permite ajustar el volumen de efectos desde el Inspector en Play Mode.
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    // ---------------- API pública ----------------

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySfx(buttonClick);

    // ---------------- Enlace con cada escena ----------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => BindScene();

    private void BindScene()
    {
        // Si se salió de la pausa al menú, la música no debe quedarse baja o detenida.
        SetMusicPaused(false);
        if (newRecordRoutine != null) { StopCoroutine(newRecordRoutine); newRecordRoutine = null; }

        BindGameManager();
        BindButtons();
    }

    private void BindGameManager()
    {
        UnbindGameManager();
        // GameManager.Instance se asigna en su Awake, que corre antes de sceneLoaded.
        gm = GameManager.Instance;
        if (gm == null) return;

        gm.FruitCaught += OnFruitCaught;
        gm.LifeLost += OnLifeLost;
        gm.GameOver += OnGameOver;
        gm.PauseChanged += SetMusicPaused;
    }

    private void UnbindGameManager()
    {
        if (gm == null) return;
        gm.FruitCaught -= OnFruitCaught;
        gm.LifeLost -= OnLifeLost;
        gm.GameOver -= OnGameOver;
        gm.PauseChanged -= SetMusicPaused;
        gm = null;
    }

    /// <summary>
    /// Añade el sonido de clic a todos los botones de la escena, incluidos los de paneles
    /// ocultos (pausa, Game Over, instrucciones). Los botones son nuevos en cada carga,
    /// así que el listener nunca se duplica.
    /// </summary>
    private void BindButtons()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (var b in buttons)
            b.onClick.AddListener(PlayButtonClick);
    }

    // ---------------- Reacciones a eventos del GameManager ----------------

    private void OnFruitCaught(int points) => PlaySfx(catchFruit);

    private void OnLifeLost(GameManager.LifeLossCause cause) =>
        PlaySfx(cause == GameManager.LifeLossCause.BombCaught ? bomb : loseLife);

    private void OnGameOver()
    {
        PlaySfx(gameOver);
        if (gm != null && gm.IsNewHighScore && newRecord != null)
            newRecordRoutine = StartCoroutine(PlayDelayed(newRecord, newRecordDelay));
    }

    private IEnumerator PlayDelayed(AudioClip clip, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        PlaySfx(clip);
        newRecordRoutine = null;
    }

    private void SetMusicPaused(bool paused)
    {
        if (pauseMusicWhenPaused)
        {
            musicDucked = false;
            if (paused) musicSource.Pause();
            else if (!musicSource.isPlaying && musicSource.clip != null) musicSource.UnPause();
        }
        else
        {
            musicDucked = paused;
            if (!musicSource.isPlaying && musicSource.clip != null) musicSource.UnPause();
        }
    }
}
