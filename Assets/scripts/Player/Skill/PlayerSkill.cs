using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using TMPro;
using Unity.VisualScripting;

public class PlayerSkill : MonoBehaviour
{
    [SerializeField]
    private TMP_Text SoulStackText;

    [Header("소울 UI 참조")]
    [SerializeField] private SoulUI soulUI; // ★ Canvas의 SoulUI 스크립트 연결

    [Header("소울 시스템")]
    [SerializeField]
    private int maxSoul = 12; // 최대 소울 12

    [SerializeField]
    private int currentSoul = 12; // 현재 소울

    [SerializeField]
    private int skillSoulCost = 4; // 스킬 사용 시 소모 소울 (4 = 최대 3번 사용)

    [SerializeField]
    private bool InfiniteSoulStack = false;

    [Header("플레이어 체력 참조")]
    // [참고] 사용 중이신 체력 스크립트 타입으로 변경하여 연결해주세요.
    // 예: [SerializeField] private PlayerHealth playerHealth;

    [Header("스킬 해금 여부")]
    [SerializeField] private bool UnlockSpinSlash;
    [SerializeField] private bool UnlockDashAttack;
    [SerializeField] private bool UnlockChargeAttack;
    [SerializeField] private bool UnlockFocus;
    [SerializeField] private bool UnlockFireSpirits;
    [SerializeField] private bool UnlockFallAttack;
    [SerializeField] private bool UnlockExplodeAttack;
    [SerializeField] private bool UnlockDash;
    [SerializeField] private bool UnlockWallJump;
    [SerializeField] private bool UnlockDoubleJump;
    [SerializeField] private bool UnlockCrystalDash = false;

    [Header("컴포넌트 및 트랜스폼")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private float JumpPower;

    private bool WithUpArrow;
    private bool WithDownArrow;

    [SerializeField] private float ChargeThreshould = 0.5f; // 차지에 필요한 시간
    [SerializeField] private bool isChargingX;
    private float ChargeTimer = 0f;

    [SerializeField] private GameObject SpinSlashPrefab;

    [Header("대시 설정")]
    [SerializeField] private float DashSpeed = 10.0f;
    [SerializeField] private KeyCode DashKey = KeyCode.C;
    public bool isDash = false;
    [SerializeField] private float DashCoolDown = 1f;
    [SerializeField] private bool CanDash;
    private float DashTimer = 0f;

    [Header("스펠 / 특수기 설정")]
    [SerializeField] private GameObject FireSpiritPrefab;
    [SerializeField] private KeyCode SoulKey = KeyCode.A;
    [SerializeField] private float FireCoolDown = 0.5f;
    private float FireTimer;
    private bool canFire = true;
    public bool isFired = false;

    public bool isFallAttacking = false;
    [SerializeField] private GameObject FallAttackPrefab;
    [SerializeField] private float fallSpeed;
    [SerializeField] private float fallAttackHight;
    [SerializeField] private Transform FootPoint;
    [SerializeField] private LayerMask groundMask;

    [Header("포커스 (체력 회복) 설정")]
    [SerializeField] private float SoulchargeTime = 1.5f; // 포커스 완료에 필요한 시간
    private float SoulChargeTimer = 0f;
    public bool isChargingSoul = false;

    [SerializeField] private GameObject SouleExplosionPrefab;
    private float SoulExplosionTimer = 1f;
    [SerializeField] private float SoulExplosionCoolDown = 1f;

    [Header("크리스탈 대시")]
    [SerializeField] private float CryDashChargeTime = 2f;
    private float CryDashChargeTimer = 0f;
    [SerializeField] private float CryDashSpeed = 15f;
    public bool isCryDashing = false;
    public bool isChargingCryDash = false;
    private float CrydashDir;

    [Header("사운드")]
    [SerializeField] private AudioSource skillAudioSource;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip fireSpiritSound;


    private void Start()
    {
        currentSoul = maxSoul;

        // ★ 게임 시작 시 SoulUI 초기화
        if (soulUI != null)
        {
            soulUI.InitSoulUI(currentSoul);
        }
    }

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

        // 소울 텍스트 표시 업데이트
        if (SoulStackText != null)
        {
            SoulStackText.text = $"Soul = {currentSoul / 4} / {maxSoul / 4}";
        }

        if (Input.GetKeyDown(DashKey) && CanDash)
        {
            Dash();
        }

        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(SoulKey) && movement.GetIsGrounded() == false && hit.distance > fallAttackHight)
        {
            FallAttack();
        }

        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(SoulKey) && Time.time - SoulExplosionTimer > SoulExplosionCoolDown)
        {
            SoulExplosion();
            SoulExplosionTimer = Time.time;
        }

        if (Input.GetKey(KeyCode.S) && !isCryDashing && UnlockCrystalDash)
        {
            if (!movement.GetIsGrounded() && !movement.GetIsWallslide())
            {
                return;
            }

            isChargingCryDash = true;
            CryDashChargeTimer += Time.deltaTime;

            if (CryDashChargeTimer > 0.2f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            if (isChargingCryDash && CryDashChargeTimer >= CryDashChargeTime)
            {
                CrystalDash();
            }

            isChargingCryDash = false;
            CryDashChargeTimer = 0f;
        }

        float dir = sr.flipX ? 1f : -1f;

        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, new Vector2(dir, 0f), 0.5f, groundMask);

        if (wallHit.collider != null && isCryDashing)
        {
            rb.gravityScale = 1f;
            rb.linearVelocity = Vector2.zero;
            isCryDashing = false;
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

    #region 소울 수급 및 스킬 소모 체크

    // 적을 공격했을 때 외부(공격 스크립트)에서 호출하는 소울 획득 함수
    public void AddSoul(int amount = 1)
    {
        currentSoul = Mathf.Min(currentSoul + amount, maxSoul);

        // ★ 소울 수급 시 UI 애니메이션 갱신
        if (soulUI != null)
        {
            soulUI.UpdateSoulUI(currentSoul);
        }
    }

    // 소울 소모 가능 여부 체크 및 차감
    private bool HasEnoughSoul(int cost)
    {
        if (InfiniteSoulStack) return true;
        return currentSoul >= cost;
    }

    private void ConsumeSoul(int cost)
    {
        if (!InfiniteSoulStack)
        {
            currentSoul = Mathf.Max(0, currentSoul - cost);

            // ★ 소울 소모 시 UI 애니메이션 갱신
            if (soulUI != null)
            {
                soulUI.UpdateSoulUI(currentSoul);
            }
        }
    }

    #endregion

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

    // 소울 집중(포커스) 및 원거리 공격(발사)
    void IsChargedSoul()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            return;
        }

        if (Input.GetKeyDown(SoulKey))
        {
            SoulChargeTimer = 0;
        }

        // 소울 차징 키 누르고 있는 중 (Focus: 체력 회복)
        if (Input.GetKey(SoulKey) && movement.GetIsGrounded() && UnlockFocus)
        {
            // 소울이 부족하면 집중 불가능 (소울 4 소모)
            if (!HasEnoughSoul(skillSoulCost))
            {
                isChargingSoul = false;
                return;
            }

            isChargingSoul = true;
            SoulChargeTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            // 일정 시간 동안 집중을 마치면 소울 4 소모 + 체력 2 회복
            if (SoulChargeTimer >= SoulchargeTime)
            {
                FocusHeal();
            }
        }

        // 소울 키를 짧게 뗐을 때 (원거리 공격 실행)
        if (Input.GetKeyUp(SoulKey))
        {
            if (SoulChargeTimer < 0.2f && canFire)
            {
                if (HasEnoughSoul(skillSoulCost))
                {
                    FireSpirit();
                }
            }

            isChargingSoul = false;
            SoulChargeTimer = 0;
        }
    }

    // 소울 4를 소모하여 체력 1 회복
    private void FocusHeal()
    {
        ConsumeSoul(skillSoulCost);

        // 체력 스크립트에 접근하여 체력 2 회복 호출
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.Heal(1);
        }

        SoulChargeTimer = 0f;
        isChargingSoul = false;
    }


    void SpinSlash()
    {
        if (!UnlockSpinSlash || !HasEnoughSoul(skillSoulCost)) return;

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

            ConsumeSoul(skillSoulCost);
        }
    }


    void DashAttack()
    {
        if (!UnlockDashAttack || !UnlockDash || !HasEnoughSoul(skillSoulCost)) return;

        float dir = sr.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * DashSpeed, 0f);

        ConsumeSoul(skillSoulCost);
    }


    void ChargedAttack()
    {
        if (!UnlockChargeAttack || !HasEnoughSoul(skillSoulCost)) return;

        if (!IsChargedX())
        {

        }

        ConsumeSoul(skillSoulCost);
    }


    void Dash()
    {
        if (!UnlockDash) return;

        DashTimer = 0f;
        CanDash = false;
        isDash = true;

        float dir = sr.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * DashSpeed, 0f);

        if (skillAudioSource != null && dashSound != null)
        {
            skillAudioSource.PlayOneShot(dashSound);
        }
    }


    void FireSpirit()
    {
        if (!UnlockFireSpirits || !HasEnoughSoul(skillSoulCost)) return;

        FireTimer = 0f;
        canFire = false;
        if (!movement.GetIsGrounded())
        {
            isFired = true;
        }

        float dir = sr.flipX ? 1f : -1f;

        float PosX = rb.transform.position.x + dir * 1f;
        Vector2 finalPos = new Vector2(PosX, rb.transform.position.y);

        GameObject FireSpirt = Instantiate(FireSpiritPrefab, finalPos, Quaternion.identity);

        if (isFired)
        {
            rb.linearVelocity = new Vector2(-dir * 8f, 0f);
        }

        ConsumeSoul(skillSoulCost);

        if (skillAudioSource != null && fireSpiritSound != null)
        {
            skillAudioSource.PlayOneShot(fireSpiritSound);
        }
    }


    void FallAttack()
    {
        if (!UnlockFallAttack || !HasEnoughSoul(skillSoulCost)) return;

        isFallAttacking = true;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.down * fallSpeed, ForceMode2D.Impulse);

        ConsumeSoul(skillSoulCost);
    }


    void SoulExplosion()
    {
        if (!UnlockExplodeAttack || !HasEnoughSoul(skillSoulCost)) return;

        rb.linearVelocity = Vector2.zero;

        Vector2 InstantPos = new Vector2(transform.position.x, transform.position.y + 1f);
        GameObject SoulExplosion = Instantiate(SouleExplosionPrefab, InstantPos, Quaternion.identity);

        ConsumeSoul(skillSoulCost);
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
            if (((1 << collision.gameObject.layer) & groundMask) != 0)
            {
                rb.gravityScale = 1f;
                rb.linearVelocity = Vector2.zero;
                isCryDashing = false;
            }
        }
    }
}