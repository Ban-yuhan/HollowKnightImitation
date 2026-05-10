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
        Jump,
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

    [SerializeField]
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

    [SerializeField]
    private string ParamJumpAttackUP = "JumpAttackUP";

    [SerializeField]
    private string ParamLand = "Land";  


    private void Start()
    {
       currentHP = MaxHP;

        StartCoroutine(BossLoop());
    }


    IEnumerator BossLoop()
    {
        while (true)
        {
            if (!isExcuting)
            {
                yield return StartCoroutine(ExecutePattern(patternList[currentPatternIndex]));

                currentPatternIndex = (currentPatternIndex + 1) % patternList.Count;

                yield return new WaitForSeconds(3f);
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
                flipX();
                yield return StartCoroutine(AnticipateJump());
                break;
            case BossPatterns.Jump:
                flipX();
                yield return StartCoroutine(AnticipateJump());
                break;
        }
    }


    IEnumerator Dash()
    {
        yield return null;
    }


    IEnumerator AnticipateJump()
    {
        animator.SetTrigger(ParamAnticipateJump);
        
        yield return null;
        
        
    }

    IEnumerator Jump()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);

        if (patternList[currentPatternIndex] == BossPatterns.JumpAttack)
        {
            animator.SetTrigger(ParamJumpAttackUP);
            yield return StartCoroutine(jumpAttack());
        }
        else if (patternList[currentPatternIndex] == BossPatterns.Jump)
        {

            animator.SetTrigger(ParamJump);
            float targetX = transform.position.x + Random.Range(-10, 10);

            while (true)
            {
                float nextX = Mathf.MoveTowards(transform.position.x, targetX, MoveSpeed * Time.deltaTime);

                nextX = Mathf.Clamp(nextX, 31, 56);

                transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

                if (rb.linearVelocity.y < 0)
                {
                    RaycastHit2D hit = Physics2D.Raycast(Footpoint.position, Vector2.down, 10f, GroundMask);

                    if (hit.collider != null)
                    {
                        float distanceToGround = hit.distance;


                        if (distanceToGround < 0.25f)
                        {

                            animator.SetTrigger(ParamLand);
                            rb.linearVelocity = Vector2.zero;
                            break;
                        }
                    }
                }
                yield return null;
            }
            isExcuting = false;
        }
        yield return new WaitForSeconds(1f);
    }


    IEnumerator jumpAttack()
    {
        
        float targetX = Player.position.x > transform.position.x ? Player.position.x - 3f : Player.position.x + 3f;

        bool animationTriggered = false;

        while (true)
        {
            float nextX = Mathf.MoveTowards(transform.position.x, targetX, MoveSpeed * Time.deltaTime);

            transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

            RaycastHit2D hit = Physics2D.Raycast(Footpoint.position, Vector2.down, 10f, GroundMask);



            if (hit.collider != null)
            {
                float distanceToGround = hit.distance;

                if (distanceToGround < 2f && rb.linearVelocity.y < -0.1f && !animationTriggered)
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

        isExcuting = false;
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
