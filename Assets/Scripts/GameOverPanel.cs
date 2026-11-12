using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel de fin de partida. Aparece al recibir el evento GameOver y muestra el motivo,
/// la puntuación final, el récord y el aviso de "¡Nuevo récord!".
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private GameObject newRecordLabel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
        if (panel != null) panel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(SceneLoader.ReloadCurrent);
        if (menuButton != null) menuButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.MainMenu));

        if (gm != null) gm.GameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        if (gm != null) gm.GameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        if (reasonText != null)
            reasonText.text = gm.EndReason == GameManager.GameOverReason.TimeUp
                ? "¡Se acabó el tiempo!"
                : "¡Te quedaste sin vidas!";

        if (finalScoreText != null) finalScoreText.text = $"Puntuación: {gm.Score}";
        if (highScoreText != null) highScoreText.text = $"Récord: {gm.HighScore}";
        if (newRecordLabel != null) newRecordLabel.SetActive(gm.IsNewHighScore);

        if (panel != null) panel.SetActive(true);
        if (restartButton != null) restartButton.Select();
    }
}
