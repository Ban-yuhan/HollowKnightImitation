
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float MoveSpeed = 1.0f;

    [SerializeField]
    private int maxHealth = 10;

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

    private bool isAlive;

    private bool isGrounded;

    private int currentHealth;



    private Rigidbody2D rb;
    private void Start()
    {
        currentHealth = maxHealth;
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
    }

    private void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2((MoveSpeed * x), rb.linearVelocity.y);
    }

    void Jump()
    {
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

        if (RemainJumpTimes > 0 && canJump == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.0f);
            rb.AddForce(new Vector2(0.0f, JumpPower), ForceMode2D.Impulse);

            --RemainJumpTimes;
        }
        else
        {
            canJump = false;
        }
    }

    void Attack()
    {
        if (VerticalattackEffectPrefab != null && (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)))
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
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

                Vector2 trans = new Vector2(gameObject.transform.position.x + (1f * dirX), gameObject.transform.position.y);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX*0.5f, 0.5f, 0.5f);
                Destroy(Effect, 0.15f);
            }
            else if (sr.flipX == false)
            {
                dirX = 1f;

                Vector2 trans = new Vector2(gameObject.transform.position.x + (1f * dirX), gameObject.transform.position.y);
                GameObject Effect = Instantiate(NormalattackEffectPrefab, trans, Quaternion.identity);
                Effect.transform.localScale = new Vector3(dirX*0.5f, 0.5f, 0.5f);
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
}