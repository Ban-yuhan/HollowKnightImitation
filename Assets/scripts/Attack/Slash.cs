using UnityEngine;
using System;

public class Slash : MonoBehaviour
{
    [SerializeField]
    private int damage = 1;

    private PlayerMovement movement;
    private PlayerSkill playerSkill;

    private float knockbackForce = 1.5f;

    public static event Action OnHitSuccess;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            movement = player.GetComponent<PlayerMovement>();
            playerSkill = player.GetComponent<PlayerSkill>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적이나 가시가 아니면 무시
        if (collision.CompareTag("Enemy") == false && !collision.CompareTag("ReturnThorn"))
        {
            return;
        }

        // 1. 가시(ReturnThorn)를 쳤을 때 -> 소울 충전 안 함
        if (collision.CompareTag("ReturnThorn"))
        {
            OnHitSuccess?.Invoke();
            return; // 가시 처리 후 종료
        }

        // 2. 적(Enemy)을 쳤을 때
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);

            // 진짜 적을 때렸을 때만 소울 +1
            if (playerSkill != null)
            {
                playerSkill.AddSoul(1);
            }

            if (movement != null && movement.isDownAttack == false)
            {
                movement.HitSucess = true;
                float dirX = collision.transform.position.x - transform.position.x > 0 ? -1 : 1;
                movement.ApplyKnockback(knockbackForce * dirX);
            }

            OnHitSuccess?.Invoke();
        }
    }
}