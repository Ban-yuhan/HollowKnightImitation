using UnityEngine;

public class ShockWaveAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 2;

    [SerializeField]
    private float KnockbackForce = 3f;

    [SerializeField]
    private PlayerMovement playerMovement;

    private void Start()
    {
        if(playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
            playerMovement.isKnockbacked = true;
            playerMovement.ApplyKnockback(knockbackForce);
            damageable.TakeDamage(damage);
        }
    }
}
