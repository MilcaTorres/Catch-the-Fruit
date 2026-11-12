using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mueve la canasta horizontalmente con A/D o las flechas izquierda/derecha,
/// limitándola a los bordes visibles de la cámara ortográfica.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BasketController : MonoBehaviour
{
    [Tooltip("Velocidad horizontal en unidades por segundo.")]
    [SerializeField] private float speed = 8f;

    [Tooltip("Cámara usada para calcular los bordes. Si está vacía se usa Camera.main.")]
    [SerializeField] private Camera targetCamera;

    private Rigidbody2D rb;
    private float halfWidth;
    private float input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // Mitad del ancho de la canasta para que no se salga a medias del borde.
        var col = GetComponent<Collider2D>();
        halfWidth = col != null ? col.bounds.extents.x : 0.5f;
    }

    private void Update()
    {
        input = 0f;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input += 1f;
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        pos.x += input * speed * Time.fixedDeltaTime;

        if (targetCamera != null && targetCamera.orthographic)
        {
            float camHalfWidth = targetCamera.orthographicSize * targetCamera.aspect;
            float camX = targetCamera.transform.position.x;
            pos.x = Mathf.Clamp(pos.x, camX - camHalfWidth + halfWidth, camX + camHalfWidth - halfWidth);
        }

        rb.MovePosition(pos);
    }
}
