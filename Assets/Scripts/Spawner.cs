using System.Collections;
using UnityEngine;

/// <summary>
/// Genera objetos que caen en posiciones X aleatorias dentro del ancho de la cámara,
/// usando una Coroutine con intervalo configurable.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Tooltip("Prefabs de frutas que se pueden generar.")]
    [SerializeField] private GameObject[] fruitPrefabs;

    [Tooltip("Prefab de la bomba (se activará en otra etapa).")]
    [SerializeField] private GameObject bombPrefab;

    [Tooltip("Si es verdadero, también se generan bombas.")]
    [SerializeField] private bool spawnBombs = false;

    [Tooltip("Probabilidad (0-1) de generar una bomba cuando spawnBombs está activo.")]
    [Range(0f, 1f)]
    [SerializeField] private float bombChance = 0.2f;

    [Tooltip("Segundos entre cada objeto generado.")]
    [SerializeField] private float spawnInterval = 1f;

    [Tooltip("Margen horizontal para que los objetos no aparezcan cortados en el borde.")]
    [SerializeField] private float horizontalPadding = 0.6f;

    [Tooltip("Cámara usada para calcular los bordes. Si está vacía se usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    private Coroutine spawnRoutine;

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
        var wait = new WaitForSeconds(spawnInterval);
        while (true)
        {
            SpawnOne();
            yield return wait;
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
        Instantiate(prefab, pos, Quaternion.identity);
    }

    private GameObject PickPrefab()
    {
        if (spawnBombs && bombPrefab != null && Random.value < bombChance)
            return bombPrefab;

        if (fruitPrefabs == null || fruitPrefabs.Length == 0) return null;
        return fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
    }
}
