using TMPro;
using UnityEngine;

/// <summary>
/// HUD de la partida: puntuación, vidas y cronómetro. Solo se actualiza cuando el
/// GameManager dispara ScoreChanged / LivesChanged / TimeChanged (sin leer en Update).
/// </summary>
public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timerText;

    [Tooltip("Segundos restantes a partir de los cuales el cronómetro se pinta de este color.")]
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.3f);

    private GameManager gm;
    private Color timerNormalColor;
    private int lastShownSeconds = -1;

    // Se suscribe en Start (no en OnEnable) porque GameManager.Instance se asigna en Awake
    // y el orden de Awake/OnEnable entre objetos no está garantizado.
    private void Start()
    {
        gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[GameHUD] No hay GameManager en la escena.");
            return;
        }

        if (timerText != null) timerNormalColor = timerText.color;

        gm.ScoreChanged += OnScoreChanged;
        gm.LivesChanged += OnLivesChanged;
        gm.TimeChanged += OnTimeChanged;

        // Pinta el estado actual por si GameManager.Start ya emitió los eventos iniciales.
        OnScoreChanged(gm.Score);
        OnLivesChanged(gm.Lives);
        OnTimeChanged(gm.TimeRemaining);
    }

    private void OnDestroy()
    {
        if (gm == null) return;
        gm.ScoreChanged -= OnScoreChanged;
        gm.LivesChanged -= OnLivesChanged;
        gm.TimeChanged -= OnTimeChanged;
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = $"Puntos: {score}";
    }

    private void OnLivesChanged(int lives)
    {
        if (livesText != null) livesText.text = $"Vidas: {lives}";
    }

    private void OnTimeChanged(float remaining)
    {
        if (timerText == null) return;

        // TimeChanged llega cada frame; solo se reconstruye el texto cuando cambia el segundo.
        int seconds = Mathf.CeilToInt(remaining);
        if (seconds == lastShownSeconds) return;
        lastShownSeconds = seconds;

        timerText.text = $"{seconds / 60}:{seconds % 60:00}";
        timerText.color = remaining <= warningTime ? warningColor : timerNormalColor;
    }
}
