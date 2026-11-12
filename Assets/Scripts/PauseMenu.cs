using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Panel de pausa. Escape o P alternan la pausa; el GameManager pone Time.timeScale = 0
/// y avisa con PauseChanged, que es lo que muestra u oculta el panel.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
        if (panel != null) panel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(() => gm?.Resume());
        if (menuButton != null) menuButton.onClick.AddListener(() => SceneLoader.Load(SceneLoader.MainMenu));

        if (gm != null) gm.PauseChanged += OnPauseChanged;
    }

    private void OnDestroy()
    {
        if (gm != null) gm.PauseChanged -= OnPauseChanged;
    }

    // Update sigue corriendo con timeScale = 0, así que sirve para leer la tecla de reanudar.
    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || gm == null) return;

        if (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
            gm.TogglePause(); // No hace nada si la partida ya terminó.
    }

    private void OnPauseChanged(bool paused)
    {
        if (panel != null) panel.SetActive(paused);
        if (paused && resumeButton != null) resumeButton.Select();
    }
}
