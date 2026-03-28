
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

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
    private KeyCode AttackKey = KeyCode.Q;

    [SerializeField]
    private GameObject NormalattackEffectPrefab;

    [SerializeField]
    private GameObject VerticalattackEffectPrefab;

    private SpriteRenderer sr;

    private int RemainJumpTimes;

    private bool canJump;

    private bool isGrounded;

    private Rigidbody2D rb;

    private float lastAttackTime;
    private float attackBufferTime = 0.2f; // 공격 후 0.2초 안에 적을 맞추면 반동 인정

    private void Start()
    {
        RemainJumpTimes = MaxJumpTimes;
        canJump = true;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        PlayerHead();

        if (Input.GetKeyDown(JumpKey))
        {
            Jump();
        }

        if (Input.GetKeyDown(AttackKey))
        {
            Attack();
        }

        if (FootPoint != null)
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2)FootPoint.position, Vector2.down, 0.2f, groundMask);

            if (hit.collider != null)
            {
                isGrounded = true;
                RemainJumpTimes = MaxJumpTimes;
                canJump = true;
            }
            else
            {
                isGrounded = false;
            }
        }
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2((MoveSpeed * x), rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (RemainJumpTimes > 0 && canJump == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.0f);
            rb.AddForce(new Vector2(0.0f, JumpPower), ForceMode2D.Impulse);

            --RemainJumpTimes;
        }
        else if(RemainJumpTimes <= 0)
        {
            canJump = false;
        }
    }

    void Attack()
    {
        if (VerticalattackEffectPrefab != null && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)))
        {
            if (Input.GetKey(KeyCode.DownArrow) && !isGrounded)
            {
                lastAttackTime = Time.time; // 공격한 시점을 기록

                Vector2 trans = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - 0.7f);
                GameObject Effect = Instantiate(VerticalattackEffectPrefab, trans, Quaternion.Euler(0, 0, 0));
                Destroy(Effect, 0.15f);
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                Vector2 trans = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + 0.7f);
                GameObject Effect = Instantiate(VerticalattackEffectPrefab, trans, Quaternion.Euler(0, 0, 180));
                Destroy(Effect, 0.15f);
            }

        }
        else if(NormalattackEffectPrefab != null)
        {
            float dirX = 1f;

            if (sr.flipX == true)
            {
                dirX = -1f;

                Vector2 trans = new Vector2(gameObject.transform.position.x + (0.7f * dirX), gameObject.transform.position.y - 0.2f);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX* Effect.transform.localScale.x, Effect.transform.localScale.y, Effect.transform.localScale.z);
                Destroy(Effect, 0.15f);
            }
            else if (sr.flipX == false)
            {
                dirX = 1f;

                Vector2 trans = new Vector2(gameObject.transform.position.x + (0.7f * dirX), gameObject.transform.position.y-0.2f);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX*Effect.transform.localScale.x, Effect.transform.localScale.y, Effect.transform.localScale.z);
                Destroy(Effect, 0.15f);
            }
        }
        
    }

    void PlayerHead()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            sr.flipX = true;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            sr.flipX = false;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
        }
    }
}