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
    private bool consumed;

    /// <summary>
    /// Marca el objeto como procesado. Devuelve false si ya lo procesó otro trigger
    /// (Destroy es diferido, así que evita contar dos veces canasta + suelo en el mismo frame).
    /// </summary>
    public bool TryConsume()
    {
        if (consumed) return false;
        consumed = true;
        return true;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        // Nota: no hace falta useFullKinematicContacts. Como el collider es Trigger, Box2D/Unity
        // sí reporta OnTriggerEnter2D contra Kinematic (canasta) y estáticos (suelo).
        // Ese flag solo afecta a colisiones sólidas (OnCollisionEnter2D).
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
