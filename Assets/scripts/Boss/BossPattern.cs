using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossPattern : MonoBehaviour
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

    [Header("보스 활성화 및 생존 상태")]
    [SerializeField] private bool isActivated = false;
    private bool isAlive = true;

    [Header("현재 상태")]
    [SerializeField] private BossState currentState = BossState.Idle;

    [Header("보스 이동 및 점프 관련 변수")]
    [SerializeField] private float JumpForce = 10f;
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float patternCooldown = 1.5f;

    [Header("맵 X축 범위")]
    [SerializeField] private float minX = 30f;
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

    public void ActivateBoss()
    {
        isActivated = true;
    }

    public void OnDie()
    {
        isAlive = false;
        isExecuting = false;

        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator FSMMainLoop()
    {
        yield return new WaitUntil(() => isActivated);

        while (isAlive)
        {
            if (!isExecuting)
            {
                LookAtPlayer();

                currentState = BossState.Idle;
                yield return new WaitForSeconds(patternCooldown);

                if (!isAlive) break;

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

    private IEnumerator PatternRandomJump()
    {
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

    private IEnumerator PatternJumpSlam()
    {
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

    private IEnumerator PatternBackJumpToBigSlam()
    {
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

    private IEnumerator PatternBigSlamToWave()
    {
        animator.SetTrigger(ParamAnticipateAttack);
        yield return new WaitForSeconds(0.8f);

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
                if (!isAlive) yield break;

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

    #region 유틸리티 함수

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