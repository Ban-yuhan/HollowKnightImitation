using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("UI 연동")]
    [SerializeField] private HealthUI healthUI; // HealthUI 스크립트 연결

    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 5; // 마스크 5개 기준
    [SerializeField] private int currentHealth;
    [SerializeField] private PlayerSkill skill;
    [SerializeField] private PlayerMovement movement;

    [Header("무적 및 피격 연출 설정")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("리턴 히트박스 / 가시 리스폰 설정")]
    [SerializeField] private string returnTag = "ReturnHitBox";
    [SerializeField] private string thornTag = "ReturnThorn";
    [SerializeField] private int hazardDamage = 1;
    [SerializeField] private float respawnFreezeDuration = 0.5f;

    private Vector3 lastGroundedPosition;
    private Rigidbody2D rb;

    private bool isAlive;
    public bool isInvulnerable = false;

    private void Start()
    {
        currentHealth = maxHealth;
        isAlive = true;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (skill == null)
            skill = GetComponent<PlayerSkill>();

        rb = GetComponent<Rigidbody2D>();

        lastGroundedPosition = transform.position;

        // 게임 시작 시 HealthUI 초기화
        if (healthUI != null)
        {
            healthUI.InitHealthUI(maxHealth);
        }
    }

    private void Update()
    {
        if (movement != null && movement.GetIsGrounded() && !isInvulnerable && movement.enabled)
        {
            lastGroundedPosition = transform.position;
        }
    }

    #region 체력 UI 업데이트

    private void UpdateHPUI()
    {
        if (healthUI != null)
        {
            healthUI.UpdateHealthUI(currentHealth);
        }
    }

    #endregion

    #region 체력 회복 (포커스 스킬 연동)

    public void Heal(int amount)
    {
        if (!isAlive) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHPUI();
        Debug.Log($"체력 회복됨! 현재 체력: {currentHealth}/{maxHealth}");
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(returnTag) || collision.CompareTag(thornTag))
        {
            RespawnAtLastSafePoint();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(returnTag) || collision.gameObject.CompareTag(thornTag))
        {
            RespawnAtLastSafePoint();
        }
    }

    private void RespawnAtLastSafePoint()
    {
        TakeDamage(hazardDamage);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        transform.position = lastGroundedPosition;

        StartCoroutine(RespawnFreezeRoutine());
    }

    private IEnumerator RespawnFreezeRoutine()
    {
        if (movement != null)
        {
            movement.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(respawnFreezeDuration);

        if (movement != null && isAlive)
        {
            movement.enabled = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || !isAlive) return;

        currentHealth -= damage;
        UpdateHPUI();

        if (skill != null && skill.isCryDashing)
        {
            skill.isCryDashing = false;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    public IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float timer = 0f;
        bool isBlack = false;

        while (timer < invulnerabilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isBlack ? Color.white : Color.black;
                isBlack = !isBlack;
            }

            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        isAlive = false;

        if (movement != null) movement.enabled = false;
        if (skill != null) skill.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Destroy(gameObject, 1f);
    }

    public bool GetisAlive() => isAlive;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}