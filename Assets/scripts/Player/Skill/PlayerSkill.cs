using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerSkill : MonoBehaviour
{

    [SerializeField]
    private bool UnlockSpinSlash;

    [SerializeField]
    private bool UnlockDashAttack;

    [SerializeField]
    private bool UnlockChargeAttack;

    [SerializeField]
    private bool UnlockFocus;

    [SerializeField]
    private bool UnlockFireSpirits;

    [SerializeField]
    private bool UnlockFallAttack;

    [SerializeField]
    private bool UnlockExplodeAttack;

    [SerializeField]
    private bool UnlockDash;

    [SerializeField]
    private bool UnlockWallJump;

    [SerializeField]
    private bool UnlockDoubleJump;

    [SerializeField]
    private PlayerMovement movement;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private SpriteRenderer sr;

    [SerializeField]
    private float JumpPower;

    private bool WithUpArrow;

    private bool WithDownArrow;

    [SerializeField]
    private float ChargeThreshould= 0.5f; //차지에 필요한 시간

    [SerializeField]
    private bool isCharging;

    private float ChargeTimer = 0f;

    [SerializeField]
    private GameObject SpinSlashPrefab;

    [SerializeField]
    private float DashSpeed = 10.0f;

    [SerializeField]
    private KeyCode DashKey = KeyCode.C;

    public bool isDash = false;

    [SerializeField]
    private float DashCoolDown = 1f;

    [SerializeField]
    private bool CanDash;

    private float DashTimer = 0f;

    [SerializeField]
    private GameObject FireSpiritPrefab;

    [SerializeField]
    private KeyCode SoulKey = KeyCode.Space;

    [SerializeField]
    private float FireCoolDown = 0.5f;

    private float FireTimer;

    private bool canFire = true;

    public bool isFired = false;

    public bool isFallAttacking = false;

    [SerializeField]
    private GameObject FallAttackPrefab;

    [SerializeField]
    private float fallSpeed;

    private void Update()
    {
        IsCharged();

        DashTimer += Time.deltaTime;
        FireTimer += Time.deltaTime;

        if(DashTimer > DashCoolDown)
        {
            CanDash = true;
        }

        if(FireTimer > FireCoolDown)
        {
            canFire = true;
        }

        if (Input.GetKeyDown(DashKey) && IsCharged())
        {
            DashAttack();
        }

        if (Input.GetKeyDown(DashKey) && CanDash)
        {
            Dash();
        }

        if (Input.GetKeyDown(SoulKey) && canFire && !Input.GetKey(KeyCode.DownArrow))
        {
            FireSpirit();
        }

        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(SoulKey) && movement.GetIsGrounded() == false)
        {
            FallAttack();
        }
    }


    private void FixedUpdate()
    {
        if (isDash)
        {
            float damping = 0.82f;
            Vector2 newVel = rb.linearVelocity;
            newVel.x *= damping;

            newVel.y = 0;
            rb.linearVelocity = newVel;

            if (Mathf.Abs(rb.linearVelocity.x) < 2f)
            {
                isDash = false;
            }
        }

        if (isFired)
        {
            float damping = 0.82f;
            Vector2 newVel = rb.linearVelocity;
            newVel.x *= damping;

            newVel.y = 0;
            rb.linearVelocity = newVel;

            if (Mathf.Abs(rb.linearVelocity.x) < 1f)
            {
                isFired = false;
            }
        }

        if (isFallAttacking)
        {
            if (movement.GetIsGrounded() == true)
            {
                GameObject FallAttack = Instantiate(FallAttackPrefab, transform.position, Quaternion.identity);
                isFallAttacking = false;
            }
        }
    }


    bool IsCharged()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            isCharging = true;
            ChargeTimer = 0;
        }

        if (Input.GetKey(KeyCode.X) && isCharging)
        {
            ChargeTimer += Time.deltaTime;

            if (ChargeTimer >= ChargeThreshould)
            {
                return true;
            }
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            isCharging = false;
            ChargeTimer = 0;
        }

        return false;
    }


    void SpinSlash()
    {
        if (!UnlockSpinSlash)
        {
            return;
        }

        if (IsCharged())
        {
            if (WithUpArrow)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0.0f);
                rb.AddForce(new Vector2(0.0f, JumpPower), ForceMode2D.Impulse);

                if (SpinSlashPrefab != null)
                {
                    GameObject Effect = Instantiate(SpinSlashPrefab, rb.transform.position, Quaternion.identity);
                    Destroy(Effect, 0.08f);
                }
            }

            else if (WithDownArrow)
            {
                GameObject Effect = Instantiate(SpinSlashPrefab, rb.transform.position, Quaternion.identity);
                Destroy(Effect, 0.08f);
            }
        }
    }


    void DashAttack()
    {
        if (!UnlockDashAttack || !UnlockDash)
        {
            return;
        }
            float dir = sr.flipX ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir*DashSpeed, 0f);
    }


    void ChargedAttack()
    {
        if (!UnlockChargeAttack)
        {
            return;
        }

        if(!IsCharged())
        {

        }
    }


    void Dash()
    {
        if (!UnlockDash)
        {
            return;
        }

        DashTimer = 0f;
        CanDash = false;
        isDash = true;

        float dir = sr.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir*DashSpeed, 0f);
    }


    void FireSpirit()
    {
        if (!UnlockFireSpirits)
        {
            return;
        }

        FireTimer = 0f;
        canFire = false;
        if (!movement.GetIsGrounded())
        {
            isFired = true;
        }

        float dir = sr.flipX ? 1f : -1f;

        float PosX = rb.transform.position.x + dir * 1f;
        Vector2 finalPos = new Vector2(PosX, rb.transform.position.y);

        GameObject FireSpirt = Instantiate(FireSpiritPrefab, finalPos , Quaternion.identity);

        if (isFired)
        {
            rb.linearVelocity = new Vector2(-dir * 8f, 0f);
        }
    }


    void FallAttack()
    {
        if (!UnlockFallAttack)
        {
            return;
        }

        isFallAttacking = true;

        rb.linearVelocity = new Vector2(0f, -fallSpeed);
    }
}
