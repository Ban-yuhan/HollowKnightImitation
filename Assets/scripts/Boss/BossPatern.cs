using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using NUnit.Framework.Constraints;

public class BossPatern : MonoBehaviour
{
    [SerializeField]
    private enum BossPatterns
    {
        JumpAttack,
        BackJump,
        WaveAttack,
        Dash,
        SideAttack
    }

    [Header("패턴 리스트")]
    [SerializeField]
    private List<BossPatterns> patternList = new List<BossPatterns>();

    [SerializeField] 
    private float MaxHP = 100f;

    private float currentHP;

    [SerializeField]
    private float JumpForce = 10f;

    [SerializeField]
    private float MoveSpeed = 5f;

    [SerializeField]
    private Transform Player;

    private int currentPatternIndex = 0;

    private bool isExcuting = false;

    [SerializeField]
    private Transform Footpoint;

    [SerializeField]
    private LayerMask GroundMask;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private SpriteRenderer sr;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string ParamJump = "Jump";

    [SerializeField]
    private string ParamJumpAttack = "JumpAttack";

    [SerializeField]
    private string ParamJumpAttack2 = "JumpAttack2";

    [SerializeField]
    private string ParamAnticipateJump = "AnticipateJump";


    private void Start()
    {
       currentHP = MaxHP;

        StartCoroutine(BossLoop());
    }

    private void Update()
    {
        if (!isExcuting)
        {
            flipX();
        }
    }

    IEnumerator BossLoop()
    {
        while (true)
        {
            if (!isExcuting)
            {
                yield return StartCoroutine(ExecutePattern(patternList[currentPatternIndex]));

                currentPatternIndex = (currentPatternIndex + 1) % patternList.Count;

                yield return new WaitForSeconds(2f);
            }
            yield return null;
        }
    }

    IEnumerator ExecutePattern(BossPatterns pattern)
    {
        isExcuting = true;
        
        switch (pattern)
        {
            case BossPatterns.JumpAttack:
                yield return StartCoroutine(JumpAttack());
                break;
        }

        isExcuting = false;
    }

    IEnumerator Dash()
    {
        float targetDir = (Player.position.x - transform.position.x);

        float TargetPos = targetDir > 0 ? targetDir + 3f : targetDir - 3f;

        while (Mathf.Abs(transform.position.x - targetDir) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(TargetPos, 0f), MoveSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = new Vector2(TargetPos, 0f);


    }


    IEnumerator JumpAttack()
    {
        animator.SetTrigger(ParamAnticipateJump);
        
        yield return null;
        
        
    }
    IEnumerator jump()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        animator.SetTrigger(ParamJump);

        float targetX = Player.position.x;

        bool animationTriggered = false;

        while (true)
        {
            float nextX = Mathf.MoveTowards(transform.position.x, targetX, MoveSpeed * Time.deltaTime);

            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

            RaycastHit2D hit = Physics2D.Raycast(Footpoint.position, Vector2.down, 10f, GroundMask);

            if (hit.collider != null)
            {
                float distanceToGround = hit.distance;

                if (rb.linearVelocity.y < -6f && !animationTriggered)
                {
                    animator.SetTrigger(ParamJumpAttack);
                    animationTriggered = true;
                }

                if (distanceToGround < 0.1f || Mathf.Abs(rb.linearVelocity.y) < 0.01f && animationTriggered)
                {
                    animator.SetTrigger(ParamJumpAttack2);
                    break;
                }
            }

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(1f);
    }




    private void flipX()
    {
        if(transform.position.x - Player.position.x > 0)
        {
            transform.localScale = new Vector3 (-1,1,1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
