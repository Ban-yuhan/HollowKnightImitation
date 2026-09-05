using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float MaxHP = 100f;
    [SerializeField] private float currentHP;

    [SerializeField]
    private int damage = 2;

    [SerializeField]
    private float KnockbackForce = 3f;

    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private GameObject Boss;

    private bool isAlive = true;


    private void Start()
    {
        currentHP = MaxHP;
        isAlive = true;
    }


    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            isAlive = false;
            Die();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == false)
        {
            return;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if(damageable == null)
        {
            return;
        }

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


    private void Die()
    {
        if (isAlive)
        {
            isAlive = false;
            Destroy(Boss, 1f);
        }
    }
}
