using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("UI 연동")]
    [SerializeField] private HealthUI healthUI;

    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    // ★ 씬 전환 후에도 체력을 유지하기 위한 static 변수
    private static int savedHealth = -1;

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

    [Header("사망 및 게임오버 설정")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float gameOverDelay = 1.0f; // 사망 연출 대기시간

    private Vector3 lastGroundedPosition;
    private Rigidbody2D rb;

    private bool isAlive = true;
    public bool isInvulnerable = false;

    private void Awake()
    {
        if (savedHealth == -1)
        {
            currentHealth = maxHealth;
            savedHealth = currentHealth;
        }
        else
        {
            currentHealth = savedHealth;
        }

        isAlive = true;
    }

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (skill == null) skill = GetComponent<PlayerSkill>();
        rb = GetComponent<Rigidbody2D>();

        lastGroundedPosition = transform.position;
    }

    private void Update()
    {
        if (movement != null && movement.GetIsGrounded() && !isInvulnerable && movement.enabled)
        {
            lastGroundedPosition = transform.position;
        }
    }

    #region 체력 UI 업데이트

    public void UpdateHPUI()
    {
        if (healthUI == null) healthUI = GameObject.FindAnyObjectByType<HealthUI>();

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
        savedHealth = currentHealth;
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

        if (rb != null) rb.linearVelocity = Vector2.zero;
        transform.position = lastGroundedPosition;

        StartCoroutine(RespawnFreezeRoutine());
    }

    private IEnumerator RespawnFreezeRoutine()
    {
        if (movement != null) movement.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(respawnFreezeDuration);

        if (movement != null && isAlive) movement.enabled = true;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || !isAlive) return;

        currentHealth -= damage;
        savedHealth = currentHealth;
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

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }

    private void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        Debug.Log("플레이어 사망! 게임 오버 처리 시작");

        // 게임 오버 후 리스폰을 위해 저장 체력 리셋
        savedHealth = maxHealth;

        // 1. 이동 컴포넌트 꺼주기
        if (movement != null) movement.enabled = false;

        // 2. 스킬 컴포넌트 꺼주기 (공격이 스킬 내부에 있다면 차단됨)
        if (skill != null) skill.enabled = false;

        // 3. 별도의 공격 스크립트가 플레이어에 붙어 있다면 그것도 꺼주기 (필요 시 타입명 변경)
        MonoBehaviour attackScript = GetComponent("PlayerAttack") as MonoBehaviour;
        if (attackScript != null) attackScript.enabled = false;

        // 4. 물리 이동 멈추기
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 5. 씬 이동 코루틴 실행
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        // 사망 효과음을 듣거나 모션을 볼 수 있도록 잠시 대기
        yield return new WaitForSeconds(gameOverDelay);

        Debug.Log("FadeManager 탐색 및 씬 전환 시도...");

        // 씬 내의 FadeManager 검색
        FadeManager fadeManager = GameObject.FindAnyObjectByType<FadeManager>();

        if (fadeManager != null)
        {
            // 가지고 계신 FadeManager에 어떤 전환 함수가 있는지 모르므로, 대표적인 메시지를 전송합니다.
            fadeManager.SendMessage("FadeOutToScene", gameOverSceneName, SendMessageOptions.DontRequireReceiver);
            fadeManager.SendMessage("FadeToScene", gameOverSceneName, SendMessageOptions.DontRequireReceiver);
            fadeManager.SendMessage("LoadScene", gameOverSceneName, SendMessageOptions.DontRequireReceiver);
            fadeManager.SendMessage("ChangeScene", gameOverSceneName, SendMessageOptions.DontRequireReceiver);

            // 혹시 FadeManager 함수명이 위와 다를 것을 대비해 1.5초 후에도 씬이 안 바뀌면 강제 전환
            yield return new WaitForSeconds(1.5f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogWarning("FadeManager를 찾을 수 없어 일반 SceneManager로 직접 전환합니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
        }
    }

    public bool GetisAlive() => isAlive;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}