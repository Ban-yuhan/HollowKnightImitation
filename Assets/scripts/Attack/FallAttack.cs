using UnityEngine;

public class FallAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 20;

    [SerializeField]
    private float duration = 0.2f;

    private void Start()
    {
        Destroy(gameObject, duration);

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") == false)
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}
