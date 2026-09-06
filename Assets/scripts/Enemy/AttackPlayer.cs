using UnityEngine;

public class AttackPlayer : MonoBehaviour
{
    [Header("기본 컴포넌트")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private Transform SensorPoint;
    [SerializeField] private LayerMask groundMask;

    [Header("플레이어 감지 설정")]
    [SerializeField] private float detectRadius = 5f;
    [SerializeField] private float fovAngle = 90f;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("공격(돌진) 설정")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float attackInterval = 0.6f;

    [Header("패트롤(순찰) 설정")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f; // 시작 위치 기준 좌우 이동 거리

    private float attackTimer = 0f;
    private Vector2 startPosition;
    private bool movingRight = true;

    private void Start()
    {
        GameObject objPlayer = GameObject.Find("TheKnight");
        if (objPlayer != null)
        {
            player = objPlayer.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 기준이 될 시작 위치 저장
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 1. 플레이어를 감지하지 못한 경우 -> 패트롤 수행
        if (!CanSeePlayer())
        {
            attackTimer = 0f;
            Patrol();
            return;
        }

        // 2. 플레이어 감지 성공 -> 공격(돌진) 수행
        AttackPattern();
    }

    private void Patrol()
    {
        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;

        // 이동 방향에 따른 스프라이트 반전 및 이동 처리
        if (movingRight)
        {
            sr.flipX = true;
            rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);

            if (transform.position.x >= rightLimit)
            {
                movingRight = false;
            }
        }
        else
        {
            sr.flipX = false;
            rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);

            if (transform.position.x <= leftLimit)
            {
                movingRight = true;
            }
        }
    }

    private void AttackPattern()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // 시선 방향 전환
        if (direction.x < 0) sr.flipX = false;
        else if (direction.x > 0) sr.flipX = true;

        attackTimer += Time.fixedDeltaTime;

        // 대기 시간이 지나면 플레이어 쪽으로 이동
        if (attackTimer >= attackInterval)
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            // 3초 후 이동 멈추고 타이머 리셋
            if (attackTimer >= 3f)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                attackTimer = 0f;
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null || SensorPoint == null || playerHealth == null) return false;
        if (!playerHealth.GetisAlive()) return false;

        Vector2 origin = SensorPoint.position;
        Vector2 playerPos = player.position;
        Vector2 toPlayer = playerPos - origin;

        float distance = toPlayer.magnitude;

        // 1. 거리 체크
        if (distance > detectRadius) return false;

        // 2. 시야각 체크 (sr.flipX 기준)
        Vector2 forward = sr.flipX ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, toPlayer);

        if (angle > fovAngle * 0.5f) return false;

        // 3. 장애물 레이캐스트
        RaycastHit2D block = Physics2D.Raycast(origin + toPlayer.normalized * 0.1f, toPlayer.normalized, distance, groundMask);

        if (block.collider != null) return false;

        return true;
    }

    // 씬 뷰에서 감지 영역 및 패트롤 범위를 쉽게 확인
    private void OnDrawGizmosSelected()
    {
        // 패트롤 범위 시각화 (청록색)
        Vector3 startPos = Application.isPlaying ? (Vector3)startPosition : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(startPos.x - patrolDistance, startPos.y, startPos.z),
                        new Vector3(startPos.x + patrolDistance, startPos.y, startPos.z));

        // 감지 원 시각화 (빨간색)
        if (SensorPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(SensorPoint.position, detectRadius);
        }
    }
}