using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float MaxHP = 100f;
    [SerializeField] private float currentHP;

    [SerializeField] private int damage = 2;
    [SerializeField] private float KnockbackForce = 3f;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject Boss;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private DoorLock doorLock;
    [SerializeField] private BossPattern bossPattern;
    [SerializeField] private CameraFollow2D cameraFollow;

    [Header("스프라이트 및 피격 플래시 연출")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private bool isAlive = true;

    [Header("애니메이션 파라미터")]
    [SerializeField] private string ParamDie = "Die";

    [Header("씬 전환 설정")]
    [SerializeField] private string clearSceneName = "ClearScene";
    [SerializeField] private float deathDelay = 3f; // 사망 애니메이션 대기 시간

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (bossPattern == null) bossPattern = GetComponent<BossPattern>();
        if (doorLock == null) doorLock = GameObject.FindAnyObjectByType<DoorLock>();
        if (cameraFollow == null) cameraFollow = GameObject.FindAnyObjectByType<CameraFollow2D>();

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        currentHP = MaxHP;
        isAlive = true;
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive) return;

        currentHP -= damage;
        FlashWhite();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void FlashWhite()
    {
        if (spriteRenderer == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashWhiteRoutine());
    }

    private IEnumerator FlashWhiteRoutine()
    {
        spriteRenderer.color = new Color(10f, 10f, 10f, 1f);
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAlive) return;
        if (collision.gameObject.CompareTag("Player") == false) return;

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable == null) return;

        Vector2 PlayerPos = collision.gameObject.transform.position;
        float dirX = PlayerPos.x - transform.position.x;
        float knockbackForce = KnockbackForce * (dirX > 0 ? 1 : -1);

        if (playerMovement != null)
        {
            playerMovement.isKnockbacked = true;
            playerMovement.ApplyKnockback(knockbackForce);
        }

        damageable.TakeDamage(damage);
    }

    private void Die()
    {
        isAlive = false;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;

        if (bossPattern != null) bossPattern.OnDie();
        if (animator != null) animator.SetTrigger(ParamDie);
        if (doorLock != null) doorLock.GateUp();
        if (cameraFollow != null) cameraFollow.SetisInBossRoom(false);

        StartCoroutine(BossDeathAndSceneChangeRoutine());
    }

    private IEnumerator BossDeathAndSceneChangeRoutine()
    {
        // 1. 사망 애니메이션 및 연출 대기
        yield return new WaitForSeconds(deathDelay);

        // 2. 보스 오브젝트 제거
        Destroy(Boss != null ? Boss : gameObject);

        // 3. FadeManager 싱글톤을 이용해 페이드 아웃 -> Clear 씬 이동 -> 페이드 인 한 번에 처리
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.LoadSceneWithFade(clearSceneName);
        }
        else
        {
            // 혹시 FadeManager가 없는 예외 상황 대비
            UnityEngine.SceneManagement.SceneManager.LoadScene(clearSceneName);
        }
    }
}