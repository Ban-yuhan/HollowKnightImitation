using UnityEngine;

public class DumyAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;

    [SerializeField]
    private float KnockbackForce = 3f;

    [SerializeField]
    private PlayerMovement playerMovement;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == false)
        {
            return;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        Vector2 PlayerPos = collision.gameObject.transform.position;

        float dirX = PlayerPos.x - transform.position.x;
        float knockbackForce = KnockbackForce * (dirX > 0 ? 1 : -1);

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            playerMovement.isKnockbacked = true;
            playerMovement.ApplyKnockback(knockbackForce);
        }
    }
}
