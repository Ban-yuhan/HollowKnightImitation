using UnityEngine;
using System;

public class Slash : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;


    public static event Action OnHitSuccess;


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
            OnHitSuccess.Invoke();
        }
    }
}
