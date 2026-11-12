using UnityEngine;

/// <summary>
/// Zona trigger debajo de la canasta. Si una fruta llega aquí, el jugador la dejó caer:
/// resta una vida. Las bombas que caen al suelo solo se destruyen.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Ground : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var falling = other.GetComponent<FallingObject>();
        if (falling == null || !falling.TryConsume()) return;

        var gm = GameManager.Instance;
        if (!falling.IsBomb && gm != null && gm.IsPlaying)
            gm.LoseLife(GameManager.LifeLossCause.FruitDropped);

        Destroy(falling.gameObject);
    }
}
