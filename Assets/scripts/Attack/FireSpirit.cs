using UnityEngine;
using System;
using UnityEngine.UIElements;

public class FireSpirit : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private SpriteRenderer sr;

    [SerializeField]
    private float moveSpeed = 5.0f;

    [SerializeField]
    private float duration = 3f;

    private GameObject player;

    private SpriteRenderer playerSr;

    [SerializeField]
    private int damage = 10;

    private float Dir;


    private void Start()
    {
        rb.gravityScale = 0f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            SpriteRenderer playerSr = player.GetComponent<SpriteRenderer>();
            Dir = playerSr.flipX ? 1f : -1f;

            sr.flipX = playerSr.flipX ? false : true;
        }

        Destroy(gameObject, duration);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(Dir * moveSpeed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
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
            Destroy(gameObject);
        }

        Destroy(gameObject);
    }
}
