using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using TMPro;
using Unity.VisualScripting;

public class PlayerSkill : MonoBehaviour
{
    [SerializeField]
    private TMP_Text SoulStackText;

    [SerializeField]
    private int SoulStack = 3;

    [SerializeField]
    private bool InfiniteSoulStack = false;

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
    private bool UnlockCrystalDash = false;

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
    private float ChargeThreshould = 0.5f; //차지에 필요한 시간

    [SerializeField]
    private bool isChargingX;

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
    private KeyCode SoulKey = KeyCode.A;

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

    [SerializeField]
    private float fallAttackHight;

    [SerializeField]
    private Transform FootPoint;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private float SoulchargeTime = 3f;

    private float SoulChargeTimer = 0f;

    public bool isChargingSoul = false;

    [SerializeField]
    private GameObject SouleExplosionPrefab;

    private float SoulExplosionTimer = 1f;

    [SerializeField]
    private float SoulExplosionCoolDown = 1f;

    [SerializeField]
    private float CryDashChargeTime = 2f;

    private float CryDashChargeTimer = 0f;

    [SerializeField]
    private float CryDashSpeed = 15f;

    public bool isCryDashing = false;

    public bool isChargingCryDash = false;

    private float CrydashDir;


    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(FootPoint.position, Vector2.down, 100f, groundMask);

        //IsChargedX();

        IsChargedSoul();

        DashTimer += Time.deltaTime;
        FireTimer += Time.deltaTime;

        if (DashTimer > DashCoolDown)
        {
            CanDash = true;
        }

        if (FireTimer > FireCoolDown)
        {
            canFire = true;
        }

        SoulStackText.text = "Soul = " + SoulStack.ToString();

        //if (Input.GetKeyDown(DashKey) && IsChargedX())
        //{
        //    DashAttack();
        //}

        if (Input.GetKeyDown(DashKey) && CanDash)
        {
            Dash();
        }

        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(SoulKey) && movement.GetIsGrounded() == false && hit.distance > fallAttackHight)
        {
            FallAttack();
        }

        if(Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(SoulKey) && Time.time - SoulExplosionTimer > SoulExplosionCoolDown)
        {
            SoulExplosion();
            SoulExplosionTimer = Time.time;
        }

        if (Input.GetKey(KeyCode.S) && !isCryDashing && UnlockCrystalDash)
        {

            if(!movement.GetIsGrounded() && !movement.GetIsWallslide())
            {
                return;
            }

            isChargingCryDash = true;
            CryDashChargeTimer += Time.deltaTime;

            if(CryDashChargeTimer > 0.2f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }

        if(Input.GetKeyUp(KeyCode.S))
        {
            if(isChargingCryDash && CryDashChargeTimer >= CryDashChargeTime)
            {
                CrystalDash();
            }
            
            isChargingCryDash = false;
            CryDashChargeTimer = 0f;
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
                rb.linearVelocity = Vector2.zero;
                isFallAttacking = false;
            }
        }

        if (isCryDashing)
        {
            rb.linearVelocity = new Vector2(CrydashDir * CryDashSpeed, 0f);
        }
    }


    bool IsChargedX()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            isChargingX = true;
            ChargeTimer = 0;
        }

        if (Input.GetKey(KeyCode.X) && isChargingX)
        {
            ChargeTimer += Time.deltaTime;

            if (ChargeTimer >= ChargeThreshould)
            {
                return true;
            }
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            isChargingX = false;
            ChargeTimer = 0;
        }

        return false;
    }


    void IsChargedSoul()
    {
        if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            return;
        }

        if (Input.GetKeyDown(SoulKey))
        {
            SoulChargeTimer = 0;
        }

        SoulChargeTimer += Time.deltaTime;


        if (Input.GetKey(SoulKey) && SoulChargeTimer >= 0.2f && movement.GetIsGrounded() && UnlockFocus)
        {
            if (SoulStack >= 3)
            {
                isChargingSoul = false;
                return;
            }

            isChargingSoul = true;

            rb.linearVelocity = Vector2.zero;

            if (SoulChargeTimer >= SoulchargeTime)
            {
                ChargeSoul();
            }
        }

        if (Input.GetKeyUp(SoulKey))
        {
            if (SoulChargeTimer < 0.2f)
            {
                if (!InfiniteSoulStack)
                {
                    if (SoulStack <= 0)
                    {
                        return;
                    }
                }

                if (canFire)
                {
                    FireSpirit();
                }
            }

            isChargingSoul = false;
            SoulChargeTimer = 0;
        }
    }


    void SpinSlash()
    {
        if (!UnlockSpinSlash)
        {
            return;
        }

        if (IsChargedX())
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

        --SoulStack;
    }


    void DashAttack()
    {
        if (!UnlockDashAttack || !UnlockDash)
        {
            return;
        }
            float dir = sr.flipX ? -1f : 1f;
            rb.linearVelocity = new Vector2(dir*DashSpeed, 0f);

        --SoulStack;
    }


    void ChargedAttack()
    {
        if (!UnlockChargeAttack)
        {
            return;
        }

        if(!IsChargedX())
        {

        }
     
        --SoulStack;
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

        --SoulStack;
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

        --SoulStack;
    }


    void FallAttack()
    {
        if (!InfiniteSoulStack)
        {
            if (SoulStack <= 0)
            {
                return;
            }
        }

        if (!UnlockFallAttack)
        {
            return;
        }

        isFallAttacking = true;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(Vector2.down * fallSpeed, ForceMode2D.Impulse);
        
        --SoulStack;
    }


    void SoulExplosion()
    {
        if(!UnlockExplodeAttack)
        {
            return;
        }

        if (!InfiniteSoulStack)
        {
            if (SoulStack <= 0)
            {
                return;
            }
        }

        rb.linearVelocity = Vector2.zero;

        Vector2 InstantPos = new Vector2(transform.position.x, transform.position.y + 1f);

        GameObject SoulExplosion = Instantiate(SouleExplosionPrefab, InstantPos, Quaternion.identity);

        --SoulStack;
    }


    void ChargeSoul()
    {
        if (SoulStack < 3)
        {
            ++SoulStack;
        }

        SoulChargeTimer = 0f;
    }

    void CrystalDash()
    {
        rb.gravityScale = 0f;
        isChargingCryDash = false;
        isCryDashing = true;
        CryDashChargeTimer = 0f;

        CrydashDir = sr.flipX ? 1f : -1f;

        if (movement.GetIsWallslide())
        {
            CrydashDir *= -1;
            sr.flipX = !sr.flipX;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCryDashing)
        {
            if(((1 << collision.gameObject.layer) & groundMask) != 0)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = Vector2.zero;
                isCryDashing = false;
            }
        }
    }
}
