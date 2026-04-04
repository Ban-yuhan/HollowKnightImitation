
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEditor.Experimental.GraphView;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float MoveSpeed = 1.0f;

    [SerializeField]
    private int MaxJumpTimes = 2;

    [SerializeField]
    private Transform FootPoint;

    [SerializeField]
    private float JumpPower = 3.0f;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private KeyCode JumpKey = KeyCode.Space;

    [SerializeField]
    private GameObject NormalattackEffectPrefab;

    [SerializeField]
    private GameObject VerticalattackEffectPrefab;

    private SpriteRenderer sr;

    private int RemainJumpTimes;

    private bool isGrounded;

    private Rigidbody2D rb;

    private float Lastdir;
    

    private float lastAttackTime;
    private float attackBufferTime = 0.2f; // 공격 후 0.2초 안에 적을 맞추면 반동 인정

    PlayerHealth playerHealth;

    public bool isKnockbacked = false;

    [SerializeField]
    private float knockbackdamp = 5f;

    private float knockbackVel;
 
    [SerializeField]
     private float maxFallSpeed = -20f;

    private bool isGravityReset = false;

    public bool isDownAttack = false;

    public bool HitSucess = false;

    [SerializeField]
    private PlayerAnimator animator;

    private void Start()
    {

        RemainJumpTimes = MaxJumpTimes;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 1f;

        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!isKnockbacked)
        {
            if (Input.GetKeyDown(JumpKey))
            {
                Jump();
            }

            if (!isGrounded && Input.GetKeyUp(JumpKey) && rb.linearVelocity.y > 0f)
            {
                rb.gravityScale = Mathf.Lerp(rb.gravityScale, 7f, 2f); // 점프 키에서 손을 뗄 때 중력 증가
            }
        }

        if (rb.linearVelocity.y <= 0.01 && !isGravityReset && !isGrounded)
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Lerp(rb.linearVelocity.y, 0f, 5f));
            isGravityReset = true;
        }

        if (FootPoint != null)
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2)FootPoint.position, Vector2.down, 0.1f, groundMask);

            if (hit.collider != null)
            {
                isGrounded = true;
                RemainJumpTimes = MaxJumpTimes;
                rb.gravityScale = 1f; 
                isGravityReset = false;
            }
            else
            {
                isGrounded = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if(playerHealth.GetisAlive() == false)
        {
            rb.linearVelocity = new Vector2(0f, 0f);
            rb.gravityScale = 0f;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");

        if(isKnockbacked || HitSucess)
        {

            float damp = Mathf.Exp(- knockbackdamp * Time.fixedDeltaTime);
            knockbackVel *= damp;

            x = knockbackVel;

            rb.linearVelocity = new Vector2(MoveSpeed * x, rb.linearVelocity.y);

            if (Mathf.Abs(knockbackVel) < 0.01f)
            {
                knockbackVel = 0f;
                isKnockbacked = false;
            }

            return;
        }

        rb.linearVelocity = new Vector2(MoveSpeed * x, rb.linearVelocity.y);

        if (!isKnockbacked)
        {
            PlayerHead();
        }

        Vector2 velocity = rb.linearVelocity;

        velocity.y = Mathf.Max(velocity.y, maxFallSpeed);
    }

    private void Jump()
    {
        isKnockbacked = false;

        if (RemainJumpTimes > 1)
        {
            isGravityReset = false;
            rb.gravityScale = 1f;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.0f);
            rb.AddForce(new Vector2(0.0f, JumpPower), ForceMode2D.Impulse);

            --RemainJumpTimes;
        }

        if(RemainJumpTimes <= 0)
        {
            return;
        }
    }

    void Attack()
    {
        
        if (Input.GetKey(KeyCode.DownArrow) && !isGrounded)
        {
            isDownAttack = true;
            lastAttackTime = Time.time; // 공격한 시점을 기록

            Vector2 trans = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - 1.0f);
            GameObject Effect = Instantiate(VerticalattackEffectPrefab, trans, Quaternion.Euler(0, 0, 90f));
            Destroy(Effect, 0.08f);
        }
        else if (animator.GetIsUpAttacking() == true)
        {
            Vector2 trans = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 0.7f);
            GameObject Effect = Instantiate(VerticalattackEffectPrefab, trans, Quaternion.Euler(0, 0, -90f));
            Destroy(Effect, 0.08f);
        }
        else
        {
            float dirX = 1f;

            if (sr.flipX == false)
            {
                dirX = -1f;

                Vector2 trans = new Vector2(gameObject.transform.position.x + (0.7f * dirX), gameObject.transform.position.y - 0.2f);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX * Effect.transform.localScale.x, Effect.transform.localScale.y, Effect.transform.localScale.z);
                Destroy(Effect, 0.08f);
            }
            else if (sr.flipX == true)
            {
                dirX = 1f;

                Vector2 trans = new Vector2(gameObject.transform.position.x + (0.7f * dirX), gameObject.transform.position.y - 0.2f);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX * Effect.transform.localScale.x, Effect.transform.localScale.y, Effect.transform.localScale.z);
                Destroy(Effect, 0.08f);
            }
        }
    }

    void PlayerHead()
    {
        if (rb.linearVelocity.x < 0f)
            {
                sr.flipX = false;
            }
            if (rb.linearVelocity.x > 0f)
            {
                sr.flipX = true;
            }
    }

     
    void OnEnable()
    {
        Slash.OnHitSuccess += HandlePogo; 
    }
    
    
    void OnDisable() 
    {
        Slash.OnHitSuccess -= HandlePogo; // 해제 필수!
    }


    private void HandlePogo()
    {
        bool isRecentAttack = (Time.time - lastAttackTime) <= attackBufferTime;

        if (!isGrounded && isRecentAttack)
        {
            Debug.Log("Pogo Jump!");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * JumpPower * 0.6f, ForceMode2D.Impulse);
        }
    }

    
    public void ApplyKnockback(float force)
    {
        Lastdir = sr.flipX == true ? 1 : -1;
        knockbackVel += force;
    }


    public bool GetIsGrounded()
    {
        return isGrounded;
    }
}