using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BossPatern : MonoBehaviour
{
    private enum BossState
    {
        Idle,
        RandomJump,         // 1. 점프 (랜덤 위치)
        JumpSlam,           // 2. 점프 찍기 (플레이어 전방)
        BackJumpToBigSlam,  // 3. 백점프 착지 + 큰 찍기 연계
        BigSlamToWave,      // 4. 큰 찍기 + 쇼크웨이브
        Groggy              // 추후 구현할 그로기 상태
    }

    private int lastPatternIndex = 0;

    [Header("현재 상태")]
    [SerializeField] private BossState currentState = BossState.Idle;

    [Header("보스 이동 및 점프 관련 변수")]
    [SerializeField] private float JumpForce = 10f;
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float patternCooldown = 1.5f;

    [Header("맵 X축 범위")]
    [SerializeField] private float minX = 30;
    [SerializeField] private float maxX = 57f;

    [Header("충격파 프리팹")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private Transform waveSpawnPoint;

    [Header("참조 컴포넌트")]
    [SerializeField] private Transform Player;
    [SerializeField] private Transform Footpoint;
    [SerializeField] private LayerMask GroundMask;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator animator;

    [Header("애니메이션 파라미터")]
    [SerializeField] private string ParamJump = "Jump";
    [SerializeField] private string ParamJumpAttack = "JumpAttack";
    [SerializeField] private string ParamAnticipateJump = "AnticipateJump";
    [SerializeField] private string ParamAnticipateAttack = "AnticipateAttack";
    [SerializeField] private string ParamAttack = "Attack";
    [SerializeField] private string ParamJumpAttackUp = "JumpAttackUp";
    [SerializeField] private string ParamLand = "Land";

    private bool isExecuting = false;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        StartCoroutine(FSMMainLoop());
    }

    private IEnumerator FSMMainLoop()
    {
        while (true)
        {
            if (!isExecuting)
            {
                LookAtPlayer();

                currentState = BossState.Idle;
                yield return new WaitForSeconds(patternCooldown);

                SelectRandomPattern();

                yield return StartCoroutine(ExecuteState(currentState));
            }

            yield return null;
        }
    }

    private void SelectRandomPattern()
    {
        int randomVal = Random.Range(1, 5);

        if (randomVal == lastPatternIndex)
        {
            randomVal = (randomVal % 4) + 1;
        }

        lastPatternIndex = randomVal;

        switch (randomVal)
        {
            case 1: currentState = BossState.RandomJump; break;
            case 2: currentState = BossState.JumpSlam; break;
            case 3: currentState = BossState.BackJumpToBigSlam; break;
            case 4: currentState = BossState.BigSlamToWave; break;
        }
    }

    private IEnumerator ExecuteState(BossState state)
    {
        isExecuting = true;

        switch (state)
        {
            case BossState.RandomJump:
                yield return StartCoroutine(PatternRandomJump());
                break;

            case BossState.JumpSlam:
                yield return StartCoroutine(PatternJumpSlam());
                break;

            case BossState.BackJumpToBigSlam:
                yield return StartCoroutine(PatternBackJumpToBigSlam());
                break;

            case BossState.BigSlamToWave:
                yield return StartCoroutine(PatternBigSlamToWave());
                break;
        }

        isExecuting = false;
    }

    #region FSM 패턴 상세 구현

    // 패턴 1: 점프 (랜덤 위치 이동)
    private IEnumerator PatternRandomJump()
    {
        Debug.Log("패턴 1: 점프 실행");

        animator.SetTrigger(ParamAnticipateJump);
        yield return new WaitForSeconds(0.7f);

        float startX = transform.position.x;
        float targetX = Random.Range(minX, maxX);

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float totalTime = (JumpForce / gravity) * 2f;

        rb.linearVelocity = new Vector2(0f, JumpForce);
        animator.SetTrigger(ParamJump);

        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / totalTime);

            float currentX = Mathf.Lerp(startX, targetX, progress);
            rb.position = new Vector2(currentX, rb.position.y);

            yield return null;
        }

        // 점프 직후 바로 착지 판정이 나는 것을 방지하는 체공 유예 시간 추가
        float airborneTimer = 0f;
        yield return new WaitUntil(() =>
        {
            airborneTimer += Time.deltaTime;
            return airborneTimer >= 0.15f && IsGrounded() && rb.linearVelocity.y <= 0.01f;
        });

        animator.SetTrigger(ParamLand);

        rb.linearVelocity = Vector2.zero;
        ClampPosition();
        yield return new WaitForSeconds(0.4f);
    }

    // 패턴 2: 점프 찍기 (플레이어 바로 앞, 메이스 사거리 위치)
    private IEnumerator PatternJumpSlam()
    {
        Debug.Log("패턴 2: 점프 찍기 실행");

        animator.SetTrigger(ParamAnticipateJump);
        yield return new WaitForSeconds(0.7f);

        animator.SetTrigger(ParamJumpAttackUp);

        float startX = transform.position.x;
        float offset = 2.5f;
        float targetX = (Player.position.x > transform.position.x) ? Player.position.x - offset : Player.position.x + offset;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float totalTime = (JumpForce / gravity) * 2f;

        rb.linearVelocity = new Vector2(0f, JumpForce);

        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / totalTime);

            float currentX = Mathf.Lerp(startX, targetX, progress);
            rb.position = new Vector2(currentX, rb.position.y);

            yield return null;
        }

        float airborneTimer = 0f;
        yield return new WaitUntil(() =>
        {
            airborneTimer += Time.deltaTime;
            return airborneTimer >= 0.15f && IsGrounded() && rb.linearVelocity.y <= 0.01f;
        });

        animator.SetTrigger(ParamJumpAttack);

        rb.linearVelocity = Vector2.zero;
        ClampPosition();

        yield return new WaitForSeconds(0.7f);
    }

    // 패턴 3: 백점프 착지 + 큰 찍기 연계
    private IEnumerator PatternBackJumpToBigSlam()
    {
        Debug.Log("패턴 3: 백점프 착지 + 제자리 큰 찍기 연계 실행");

        animator.SetTrigger(ParamAnticipateJump);
        yield return new WaitForSeconds(0.7f);

        animator.SetTrigger(ParamJump);

        float startX = transform.position.x;
        float backDir = -GetFacingDirection();
        float backDistance = 4.0f;

        float targetX = Mathf.Clamp(startX + (backDir * backDistance), minX, maxX);

        float currentJumpForce = JumpForce * 0.8f;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float totalTime = (currentJumpForce / gravity) * 2f;

        rb.linearVelocity = new Vector2(0f, currentJumpForce);

        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / totalTime);

            float currentX = Mathf.Lerp(startX, targetX, progress);
            rb.position = new Vector2(currentX, rb.position.y);

            yield return null;
        }

        float airborneTimer = 0f;
        yield return new WaitUntil(() =>
        {
            airborneTimer += Time.deltaTime;
            return airborneTimer >= 0.15f && IsGrounded() && rb.linearVelocity.y <= 0.01f;
        });

        animator.SetTrigger(ParamLand);

        rb.linearVelocity = Vector2.zero;
        ClampPosition();

        yield return StartCoroutine(PatternBigSlamToWave());
    }

    // 패턴 4: 큰 찍기 + 쇼크웨이브(충격파)
    private IEnumerator PatternBigSlamToWave()
    {
        Debug.Log("패턴 4: 큰 찍기 + 쇼크웨이브 실행");

        animator.SetTrigger(ParamAnticipateAttack);
        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger(ParamAttack);
        rb.linearVelocity = Vector2.zero;
        LookAtPlayer();

        if (wavePrefab != null && waveSpawnPoint != null)
        {
            float dir = GetFacingDirection();
            int waveCount = 10;
            float waveInterval = 0.02f;
            float waveSpeed = 12f;

            for (int i = 0; i < waveCount; i++)
            {
                GameObject wave = Instantiate(wavePrefab, waveSpawnPoint.position, Quaternion.identity);
                wave.transform.localScale = new Vector3(dir, 1, 1);

                Rigidbody2D waveRb = wave.GetComponent<Rigidbody2D>();
                if (waveRb != null)
                {
                    waveRb.linearVelocity = new Vector2(dir * waveSpeed, 0);
                }

                yield return new WaitForSeconds(waveInterval);
            }
        }

        ClampPosition();
        yield return new WaitForSeconds(0.8f);
    }

    #endregion

    #region 유틸리티 함수 (경계 제한 / 감지)

    private void ClampPosition()
    {
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }

    private bool IsGrounded()
    {
        if (Footpoint == null) return true;
        return Physics2D.Raycast(Footpoint.position, Vector2.down, 0.4f, GroundMask);
    }

    private void LookAtPlayer()
    {
        if (Player == null) return;

        if (Player.position.x > transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private float GetFacingDirection()
    {
        return transform.localScale.x > 0 ? 1f : -1f;
    }

    private void LateUpdate()
    {
        if (transform.position.x < minX || transform.position.x > maxX)
        {
            ClampPosition();
        }
    }

    #endregion
}