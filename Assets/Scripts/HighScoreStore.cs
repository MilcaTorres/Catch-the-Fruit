using UnityEngine;

/// <summary>
/// Guarda y lee el récord con PlayerPrefs. Es la única clase que conoce la clave,
/// así el menú y el GameManager no pueden desincronizarse.
/// </summary>
public static class HighScoreStore
{
    private const string Key = "CatchTheFruit.HighScore";

    /// <summary>Récord guardado (0 si nunca se ha jugado).</summary>
    public static int Get() => PlayerPrefs.GetInt(Key, 0);

    /// <summary>
    /// Guarda <paramref name="score"/> solo si supera el récord actual.
    /// Devuelve true si es un nuevo récord.
    /// </summary>
    public static bool TrySave(int score)
    {
        if (score <= Get()) return false;

        PlayerPrefs.SetInt(Key, score);
        PlayerPrefs.Save(); // Escribe a disco ya, por si el juego se cierra de golpe.
        return true;
    }
}
