using UnityEngine;

public class DumyAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == false)
        {
            return;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}
