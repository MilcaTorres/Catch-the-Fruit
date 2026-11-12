using System;
using UnityEngine;

/// <summary>
/// Controla el estado global de la partida: puntuación, vidas, cronómetro y si se está jugando.
/// Singleton: se accede desde cualquier script con GameManager.Instance.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, GameOver }

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Ya existe un GameManager; se destruye el duplicado.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (spawner == null)
            spawner = FindAnyObjectByType<Spawner>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
            EndGame("se acabó el tiempo");
    }

    public void StartGame()
    {
        Score = 0;
        Lives = startingLives;
        TimeRemaining = gameDuration;
        State = GameState.Playing;
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
            EndGame("sin vidas");
    }

    private void EndGame(string reason)
    {
        State = GameState.GameOver;
        if (spawner != null) spawner.enabled = false;

        Debug.Log($"[GameManager] ¡Fin de la partida ({reason})! Puntuación final: {Score}");
        GameOver?.Invoke();
    }
}
