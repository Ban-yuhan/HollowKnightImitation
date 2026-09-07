using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    [Header("소울 UI 참조")]
    [SerializeField] private SoulUI soulUI;

    [Header("소울 시스템")]
    [SerializeField] private int maxSoul = 12;
    [SerializeField] private int currentSoul = 12;
    [SerializeField] private int skillSoulCost = 4;
    [SerializeField] private bool InfiniteSoulStack = false;

    // ★ 씬 이동 후에도 소울 값을 보존하기 위한 static 데이터
    private static int savedSoul = -1;

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

    [SerializeField] private float ChargeThreshould = 0.5f;
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
    [SerializeField] private float SoulchargeTime = 1.5f;
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

    private void Awake()
    {
        // ★ 게임 최초 시작 시에는 maxSoul(12)로 초기화, 씬 이동 후에는 이전 static 소울 값 로드
        if (savedSoul == -1)
        {
            currentSoul = maxSoul;
            savedSoul = currentSoul;
        }
        else
        {
            currentSoul = savedSoul;
        }
    }

    public int GetCurrentSoul() => currentSoul;
    public int GetMaxSoul() => maxSoul;

    private void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(FootPoint.position, Vector2.down, 100f, groundMask);

        IsChargedSoul();

        DashTimer += Time.deltaTime;
        FireTimer += Time.deltaTime;

        if (DashTimer > DashCoolDown) CanDash = true;
        if (FireTimer > FireCoolDown) canFire = true;

        if (Input.GetKeyDown(DashKey) && CanDash) Dash();

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
            if (!movement.GetIsGrounded() && !movement.GetIsWallslide()) return;

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
            if (isChargingCryDash && CryDashChargeTimer >= CryDashChargeTime) CrystalDash();

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

            if (Mathf.Abs(rb.linearVelocity.x) < 2f) isDash = false;
        }

        if (isFired)
        {
            float damping = 0.82f;
            Vector2 newVel = rb.linearVelocity;
            newVel.x *= damping;
            newVel.y = 0;
            rb.linearVelocity = newVel;

            if (Mathf.Abs(rb.linearVelocity.x) < 1f) isFired = false;
        }

        if (isFallAttacking)
        {
            if (movement.GetIsGrounded())
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

    public void AddSoul(int amount = 1)
    {
        currentSoul = Mathf.Min(currentSoul + amount, maxSoul);
        savedSoul = currentSoul; // ★ 소울 변경 시 static 변수도 동기화
        UpdateSoulUI();
    }

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
            savedSoul = currentSoul; // ★ 소울 변경 시 static 변수도 동기화
            UpdateSoulUI();
        }
    }

    public void UpdateSoulUI()
    {
        if (soulUI == null) soulUI = GameObject.FindAnyObjectByType<SoulUI>();
        if (soulUI != null) soulUI.UpdateSoulUI(currentSoul);
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
            if (ChargeTimer >= ChargeThreshould) return true;
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
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) return;

        if (Input.GetKeyDown(SoulKey)) SoulChargeTimer = 0;

        if (Input.GetKey(SoulKey) && movement.GetIsGrounded() && UnlockFocus)
        {
            if (!HasEnoughSoul(skillSoulCost))
            {
                isChargingSoul = false;
                return;
            }

            isChargingSoul = true;
            SoulChargeTimer += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;

            if (SoulChargeTimer >= SoulchargeTime) FocusHeal();
        }

        if (Input.GetKeyUp(SoulKey))
        {
            if (SoulChargeTimer < 0.2f && canFire)
            {
                if (HasEnoughSoul(skillSoulCost)) FireSpirit();
            }

            isChargingSoul = false;
            SoulChargeTimer = 0;
        }
    }

    private void FocusHeal()
    {
        ConsumeSoul(skillSoulCost);

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health.Heal(1);

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
        if (!movement.GetIsGrounded()) isFired = true;

        float dir = sr.flipX ? 1f : -1f;
        float PosX = rb.transform.position.x + dir * 1f;
        Vector2 finalPos = new Vector2(PosX, rb.transform.position.y);

        GameObject FireSpirt = Instantiate(FireSpiritPrefab, finalPos, Quaternion.identity);

        if (isFired) rb.linearVelocity = new Vector2(-dir * 8f, 0f);

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