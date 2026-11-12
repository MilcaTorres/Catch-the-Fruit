using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú principal: Jugar, panel de Instrucciones y récord actual.
/// Los puntos de cada fruta se leen de sus prefabs para que el texto nunca quede desactualizado.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Serializable]
    public struct FruitInfo
    {
        public string displayName;
        public FallingObject prefab;
    }

    [SerializeField] private Button playButton;
    [SerializeField] private Button instructionsButton;
    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private TMP_Text instructionsText;
    [SerializeField] private TMP_Text highScoreText;

    [Tooltip("Frutas (y bomba) a listar en las instrucciones.")]
    [SerializeField] private FruitInfo[] fruits;

    private void Start()
    {
        Time.timeScale = 1f;

        playButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.Game));
        instructionsButton.onClick.AddListener(() => SetInstructionsVisible(true));
        closeInstructionsButton.onClick.AddListener(() => SetInstructionsVisible(false));

        if (instructionsText != null) instructionsText.text = BuildInstructions();
        if (highScoreText != null) highScoreText.text = $"Récord: {HighScoreStore.Get()}";

        SetInstructionsVisible(false);
    }

    private void SetInstructionsVisible(bool visible)
    {
        instructionsPanel.SetActive(visible);
        (visible ? closeInstructionsButton : playButton).Select();
    }

    private string BuildInstructions()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b>Controles</b>");
        sb.AppendLine("Mover: A / D  o  flechas izquierda / derecha");
        sb.AppendLine("Pausa: Esc  o  P");
        sb.AppendLine();
        sb.AppendLine("<b>Puntos</b>");
        foreach (var f in fruits)
        {
            if (f.prefab == null) continue;
            sb.AppendLine(f.prefab.IsBomb
                ? $"{f.displayName}: ¡pierdes una vida!"
                : $"{f.displayName}: {f.prefab.Points} pts");
        }
        sb.AppendLine();
        sb.Append("Si una fruta toca el suelo pierdes una vida.");
        return sb.ToString();
    }
}
