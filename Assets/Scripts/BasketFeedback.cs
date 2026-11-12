using System.Collections;
using UnityEngine;

/// <summary>
/// Efectos visuales de la canasta:
/// - Rebote de escala al atrapar algo (lo llama BasketCatcher).
/// - Parpadeo en rojo al perder una vida (escucha GameManager.LifeLost).
/// </summary>
public class BasketFeedback : MonoBehaviour
{
    [Header("Rebote")]
    [Tooltip("Escala relativa en el pico del rebote (X se ensancha, Y se aplasta).")]
    [SerializeField] private Vector2 bounceScale = new Vector2(1.2f, 0.85f);
    [Min(0.01f)] [SerializeField] private float bounceDuration = 0.18f;

    [Header("Parpadeo")]
    [SerializeField] private Color flashColor = new Color(1f, 0.25f, 0.25f);
    [Min(1)] [SerializeField] private int flashCount = 3;
    [Tooltip("Segundos de cada parpadeo (encendido + apagado).")]
    [Min(0.01f)] [SerializeField] private float flashInterval = 0.12f;

    private Vector3 baseScale;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private Coroutine bounceRoutine;
    private Coroutine flashRoutine;
    private GameManager gm;

    private void Awake()
    {
        baseScale = transform.localScale;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;
    }

    private void Start()
    {
        gm = GameManager.Instance;
        if (gm != null) gm.LifeLost += OnLifeLost;
    }

    private void OnDestroy()
    {
        if (gm != null) gm.LifeLost -= OnLifeLost;
    }

    private void OnLifeLost(GameManager.LifeLossCause cause) => Flash();

    public void Bounce()
    {
        if (bounceRoutine != null) StopCoroutine(bounceRoutine);
        bounceRoutine = StartCoroutine(BounceRoutine());
    }

    public void Flash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Vector3 peak = Vector3.Scale(baseScale, new Vector3(bounceScale.x, bounceScale.y, 1f));
        for (float t = 0f; t < bounceDuration; t += Time.deltaTime)
        {
            // 0 → 1 → 0: sube rápido al pico y vuelve.
            float k = Mathf.Sin(t / bounceDuration * Mathf.PI);
            transform.localScale = Vector3.LerpUnclamped(baseScale, peak, k);
            yield return null;
        }
        transform.localScale = baseScale;
        bounceRoutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        var half = new WaitForSeconds(flashInterval * 0.5f);
        for (int n = 0; n < flashCount; n++)
        {
            SetTint(true);
            yield return half;
            SetTint(false);
            yield return half;
        }
        flashRoutine = null;
    }

    private void SetTint(bool on)
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = on ? baseColors[i] * flashColor : baseColors[i];
    }
}
