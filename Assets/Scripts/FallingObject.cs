using UnityEngine;

/// <summary>
/// Objeto que cae (fruta o bomba). El movimiento lo controla este script
/// (el Rigidbody2D tiene gravityScale = 0) y se destruye al salir por abajo de la cámara.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class FallingObject : MonoBehaviour
{
    [Tooltip("Puntos que otorga al ser atrapado.")]
    [SerializeField] private int points = 10;

    [Tooltip("Si es verdadero, este objeto es una bomba.")]
    [SerializeField] private bool isBomb = false;

    [Tooltip("Velocidad de caída en unidades por segundo.")]
    [SerializeField] private float fallSpeed = 3f;

    public int Points => points;
    public bool IsBomb => isBomb;
    public float FallSpeed
    {
        get => fallSpeed;
        set => fallSpeed = value;
    }

    private Rigidbody2D rb;
    private Camera cam;
    private float halfHeight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        cam = Camera.main;

        var col = GetComponent<Collider2D>();
        halfHeight = col != null ? col.bounds.extents.y : 0.5f;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + Vector2.down * fallSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        if (cam == null) return;

        // Borde inferior visible de la cámara ortográfica.
        float bottom = cam.transform.position.y - cam.orthographicSize;
        if (transform.position.y + halfHeight < bottom)
            Destroy(gameObject);
    }
}
