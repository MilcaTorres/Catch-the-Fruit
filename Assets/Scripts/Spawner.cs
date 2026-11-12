using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Genera objetos que caen en posiciones X aleatorias dentro del ancho de la cámara.
/// La dificultad aumenta con el tiempo de partida (leído del GameManager):
/// se acorta el intervalo de aparición, aumenta la velocidad de caída y, pasado
/// cierto segundo, empiezan a aparecer bombas. Las frutas se eligen por peso.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Serializable]
    public struct WeightedPrefab
    {
        public GameObject prefab;
        [Tooltip("Peso relativo. Mayor peso = aparece más seguido.")]
        [Min(0f)] public float weight;
    }

    [Header("Frutas")]
    [Tooltip("Frutas que se pueden generar con su peso (probabilidad relativa).")]
    [SerializeField] private WeightedPrefab[] fruits;

    [Header("Bombas")]
    [Tooltip("Prefab de la bomba.")]
    [SerializeField] private GameObject bombPrefab;

    [Tooltip("Interruptor general: si es falso nunca se generan bombas.")]
    [SerializeField] private bool spawnBombs = true;

    [Tooltip("Segundo de partida a partir del cual pueden aparecer bombas.")]
    [Min(0f)]
    [SerializeField] private float bombStartTime = 15f;

    [Tooltip("Probabilidad (0-1) de que un objeto generado sea bomba (una vez habilitadas).")]
    [Range(0f, 1f)]
    [SerializeField] private float bombChance = 0.2f;

    [Header("Dificultad progresiva")]
    [Tooltip("Segundos de partida que tarda la dificultad en llegar al máximo.")]
    [Min(0.01f)]
    [SerializeField] private float difficultyRampDuration = 60f;

    [Tooltip("Forma de la progresión: X = tiempo normalizado (0-1), Y = dificultad (0-1).")]
    [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Tooltip("Segundos entre objetos al empezar la partida.")]
    [Min(0.05f)]
    [SerializeField] private float startSpawnInterval = 1.5f;

    [Tooltip("Segundos entre objetos con la dificultad al máximo.")]
    [Min(0.05f)]
    [SerializeField] private float minSpawnInterval = 0.5f;

    [Tooltip("Multiplicador del fallSpeed de cada prefab al empezar.")]
    [Min(0f)]
    [SerializeField] private float startSpeedMultiplier = 1f;

    [Tooltip("Multiplicador del fallSpeed de cada prefab con la dificultad al máximo.")]
    [Min(0f)]
    [SerializeField] private float maxSpeedMultiplier = 2f;

    [Header("Posición")]
    [Tooltip("Margen horizontal para que los objetos no aparezcan cortados en el borde.")]
    [SerializeField] private float horizontalPadding = 0.6f;

    [Tooltip("Cámara usada para calcular los bordes. Si está vacía se usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    private Coroutine spawnRoutine;

    /// <summary>Segundos de partida transcurridos (del GameManager si existe).</summary>
    private float Elapsed => GameManager.Instance != null ? GameManager.Instance.ElapsedTime : Time.timeSinceLevelLoad;

    /// <summary>Dificultad actual entre 0 y 1.</summary>
    public float Difficulty => Mathf.Clamp01(difficultyCurve.Evaluate(Mathf.Clamp01(Elapsed / difficultyRampDuration)));
    public float CurrentSpawnInterval => Mathf.Lerp(startSpawnInterval, minSpawnInterval, Difficulty);
    public float CurrentSpeedMultiplier => Mathf.Lerp(startSpeedMultiplier, maxSpeedMultiplier, Difficulty);
    public bool BombsEnabled => spawnBombs && bombPrefab != null && Elapsed >= bombStartTime;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnOne();
            // La espera se recalcula en cada vuelta para reflejar la dificultad actual.
            yield return new WaitForSeconds(CurrentSpawnInterval);
        }
    }

    private void SpawnOne()
    {
        GameObject prefab = PickPrefab();
        if (prefab == null || targetCamera == null) return;

        float camHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
        float camX = targetCamera.transform.position.x;
        float x = Random.Range(camX - camHalfWidth + horizontalPadding,
                               camX + camHalfWidth - horizontalPadding);

        Vector3 pos = new Vector3(x, transform.position.y, 0f);
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        // El multiplicador se aplica sobre el fallSpeed propio de cada prefab.
        if (obj.TryGetComponent(out FallingObject falling))
            falling.FallSpeed *= CurrentSpeedMultiplier;
    }

    private GameObject PickPrefab()
    {
        if (BombsEnabled && Random.value < bombChance)
            return bombPrefab;

        return PickWeightedFruit();
    }

    private GameObject PickWeightedFruit()
    {
        if (fruits == null || fruits.Length == 0) return null;

        float total = 0f;
        foreach (var f in fruits)
            if (f.prefab != null) total += Mathf.Max(0f, f.weight);
        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (var f in fruits)
        {
            if (f.prefab == null || f.weight <= 0f) continue;
            roll -= f.weight;
            if (roll <= 0f) return f.prefab;
        }

        // Por redondeo de punto flotante: devuelve la última fruta válida.
        for (int i = fruits.Length - 1; i >= 0; i--)
            if (fruits[i].prefab != null && fruits[i].weight > 0f) return fruits[i].prefab;
        return null;
    }
}
