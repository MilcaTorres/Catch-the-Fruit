using System.Collections;
using UnityEngine;

/// <summary>
/// Hace temblar la cámara un instante al perder una vida.
/// Guarda la posición de reposo antes de temblar y la restaura exacta al terminar,
/// incluso si un temblor nuevo interrumpe a otro.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [Min(0f)] [SerializeField] private float duration = 0.25f;
    [Tooltip("Desplazamiento máximo en unidades de mundo.")]
    [Min(0f)] [SerializeField] private float magnitude = 0.15f;

    private Vector3 restPosition;
    private Coroutine shakeRoutine;
    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
        if (gm != null) gm.LifeLost += OnLifeLost;
    }

    private void OnDestroy()
    {
        if (gm != null) gm.LifeLost -= OnLifeLost;
    }

    private void OnDisable()
    {
        // Si se desactiva a mitad de un temblor, la cámara vuelve a su sitio.
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
            transform.localPosition = restPosition;
        }
    }

    private void OnLifeLost(GameManager.LifeLossCause cause) => Shake();

    public void Shake()
    {
        // Solo se toma la posición de reposo si no está temblando ya; si no, se
        // guardaría una posición desplazada y la cámara quedaría movida.
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        else restPosition = transform.localPosition;

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float strength = magnitude * (1f - t / duration); // se atenúa hasta 0
            Vector2 offset = Random.insideUnitCircle * strength;
            transform.localPosition = restPosition + (Vector3)offset;
            yield return null;
        }
        transform.localPosition = restPosition;
        shakeRoutine = null;
    }
}
