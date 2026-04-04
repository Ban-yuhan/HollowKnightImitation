using UnityEngine;
using System;

public class Slash : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;

    PlayerMovement movement;

    private float knockbackForce = 1.5f;

    public static event Action OnHitSuccess;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            movement = player.GetComponent<PlayerMovement>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") == false)
        {
            return;
        }

        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            if (movement.isDownAttack == false)
            {
                movement.HitSucess = true;
                float dirX = collision.transform.position.x - transform.position.x > 0 ? -1 : 1;
                movement.ApplyKnockback(knockbackForce * dirX);
            }


            OnHitSuccess.Invoke();
        }
    }
}
