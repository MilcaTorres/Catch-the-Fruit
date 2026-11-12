using System;
using UnityEngine;

/// <summary>
/// Controla el estado global de la partida: puntuación, vidas, cronómetro y si se está jugando.
/// Singleton: se accede desde cualquier script con GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Paused, GameOver }
    public enum GameOverReason { None, TimeUp, NoLives }

    public static GameManager Instance { get; private set; }

    [Tooltip("Vidas con las que empieza la partida.")]
    [SerializeField] private int startingLives = 3;

    [Tooltip("Duración de la partida en segundos. Al llegar a 0 la partida termina.")]
    [Min(1f)]
    [SerializeField] private float gameDuration = 60f;

    [Tooltip("Spawner que se desactiva al terminar la partida. Si está vacío se busca en la escena.")]
    [SerializeField] private Spawner spawner;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public GameState State { get; private set; }
    public bool IsPlaying => State == GameState.Playing;
    public bool IsPaused => State == GameState.Paused;

    /// <summary>Por qué terminó la partida (válido tras el evento GameOver).</summary>
    public GameOverReason EndReason { get; private set; }
    /// <summary>Récord guardado en PlayerPrefs (ya actualizado si esta partida lo superó).</summary>
    public int HighScore { get; private set; }
    /// <summary>True si la partida que acaba de terminar superó el récord anterior.</summary>
    public bool IsNewHighScore { get; private set; }

    /// <summary>Segundos que quedan de partida.</summary>
    public float TimeRemaining { get; private set; }
    /// <summary>Segundos transcurridos desde que empezó la partida.</summary>
    public float ElapsedTime => gameDuration - TimeRemaining;
    public float GameDuration => gameDuration;

    // Eventos para que la UI se suscriba sin acoplarse al GameManager.
    public event Action<int> ScoreChanged;
    public event Action<int> LivesChanged;
    /// <summary>Se invoca cada frame mientras se juega con los segundos restantes.</summary>
    public event Action<float> TimeChanged;
    public event Action GameOver;
    /// <summary>Se invoca al pausar (true) o reanudar (false).</summary>
    public event Action<bool> PauseChanged;

    private void Awake()
    {
        // El GameManager vive solo en la escena Game (no usa DontDestroyOnLoad).
        // Un duplicado en la MISMA escena es un error de configuración y se elimina.
        // Si la instancia previa pertenece a otra escena (la que se está descargando al
        // reiniciar), la nueva la reemplaza en lugar de autodestruirse.
        if (Instance != null && Instance != this && Instance.gameObject.scene == gameObject.scene)
        {
            Debug.LogWarning("Ya existe un GameManager en esta escena; se destruye el duplicado.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.timeScale = 1f; // Por si la escena se cargó desde la pausa sin pasar por SceneLoader.

        if (spawner == null)
            spawner = FindAnyObjectByType<Spawner>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        TimeChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
            EndGame(GameOverReason.TimeUp);
    }

    public void StartGame()
    {
        Score = 0;
        Lives = startingLives;
        TimeRemaining = gameDuration;
        State = GameState.Playing;
        EndReason = GameOverReason.None;
        IsNewHighScore = false;
        HighScore = HighScoreStore.Get();
        Time.timeScale = 1f;
        if (spawner != null) spawner.enabled = true;

        Debug.Log($"[GameManager] Partida iniciada. Puntos: {Score} | Vidas: {Lives} | Tiempo: {gameDuration:0}s");
        ScoreChanged?.Invoke(Score);
        LivesChanged?.Invoke(Lives);
        TimeChanged?.Invoke(TimeRemaining);
    }

    public void AddScore(int amount)
    {
        if (!IsPlaying) return;

        Score += amount;
        Debug.Log($"[GameManager] +{amount} puntos. Puntos: {Score} | Vidas: {Lives}");
        ScoreChanged?.Invoke(Score);
    }

    public void LoseLife(string reason)
    {
        if (!IsPlaying) return;

        Lives = Mathf.Max(0, Lives - 1);
        Debug.Log($"[GameManager] -1 vida ({reason}). Puntos: {Score} | Vidas: {Lives}");
        LivesChanged?.Invoke(Lives);

        if (Lives == 0)
            EndGame(GameOverReason.NoLives);
    }

    /// <summary>
    /// Pausa con Time.timeScale = 0: se congelan el cronómetro (Time.deltaTime = 0),
    /// el WaitForSeconds del spawner y el FixedUpdate de canasta y objetos.
    /// </summary>
    public void Pause()
    {
        if (!IsPlaying) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        Debug.Log("[GameManager] Juego en pausa.");
        PauseChanged?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        Debug.Log("[GameManager] Juego reanudado.");
        PauseChanged?.Invoke(false);
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    private void EndGame(GameOverReason reason)
    {
        State = GameState.GameOver;
        EndReason = reason;
        if (spawner != null) spawner.enabled = false;

        IsNewHighScore = HighScoreStore.TrySave(Score);
        HighScore = HighScoreStore.Get();

        Debug.Log($"[GameManager] ¡Fin de la partida ({reason})! Puntuación final: {Score} | Récord: {HighScore}" +
                  (IsNewHighScore ? " (¡nuevo récord!)" : ""));
        GameOver?.Invoke();
    }
}
