using UnityEngine;

/// <summary>
/// Detecta los objetos que caen dentro de la canasta (trigger) y avisa al GameManager:
/// fruta = suma puntos, bomba = resta una vida. En ambos casos el objeto se destruye.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BasketCatcher : MonoBehaviour
{
    private BasketFeedback feedback;

    private void Awake()
    {
        feedback = GetComponent<BasketFeedback>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var falling = other.GetComponent<FallingObject>();
        if (falling == null || !falling.TryConsume()) return;

        var gm = GameManager.Instance;
        if (gm != null && gm.IsPlaying)
        {
            if (falling.IsBomb)
                gm.LoseLife(GameManager.LifeLossCause.BombCaught);
            else
                gm.AddScore(falling.Points);

            if (feedback != null) feedback.Bounce();
        }

        Destroy(falling.gameObject);
    }
}
