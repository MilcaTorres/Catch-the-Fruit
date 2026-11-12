using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Punto único para cambiar de escena. Restablece Time.timeScale = 1 antes de cargar
/// para que una escena nunca empiece congelada si se salió desde la pausa o el Game Over.
/// </summary>
public static class SceneLoader
{
    public const string MainMenu = "MainMenu";
    public const string Game = "Game";

    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadCurrent() => Load(SceneManager.GetActiveScene().name);
}
