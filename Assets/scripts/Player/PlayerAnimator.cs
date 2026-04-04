using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private PlayerMovement movement;

    [SerializeField]
    private string paramMoveSpeed = "Speed";

    [SerializeField]
    private string paramJumping = "Jumping";

    [SerializeField]
    private string paramFalling = "Falling";

    [SerializeField]
    private string paramAttack1 = "Attack1";

    [SerializeField]
    private string paramAttack2 = "Attack2";

    [SerializeField]
    private string paramUpAttack = "UpAttack";

    [SerializeField]
    private string paramisAttacking = "isAttacking";

    [SerializeField]
    private string paramUpAttacking = "UpAttacking";

    [SerializeField]
    private bool isAttacking = false;

    private int[] AtackLR = { 0, 1 };
    private int attackIndex = 0;

    [SerializeField]
    private bool Jumping = false;

    [SerializeField]
    private bool Falling = false;

    private KeyCode AttackKey = KeyCode.Q;

    [SerializeField]
    private float AttackCooldown = 0.5f;

    private float lastAttackTime = 0f;

    private bool canAttack = true;

    [SerializeField]
    private bool isUpAttacking = false;


    private void Awake()
    {
        if( animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if( rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if(movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }   
    }


    private void Update()
    {
        if(Time.time - lastAttackTime < AttackCooldown)
        {
            canAttack = false;
        }
        else
        {
            movement.isDownAttack = false;
            movement.HitSucess = false;
            canAttack = true;
        }

        Vector2 v = rb != null ? rb.linearVelocity : Vector2.zero;

        float speedX = v.x;

        float speedXAbs = Mathf.Abs(v.x);

        float speedY = v.y;

        if(v.y > 0f)
        {
            Jumping = true;
        }
        else
        {
            Jumping = false;
        }
        

        if(v.y <= 0f && movement.GetIsGrounded() == false)
        {
            Falling = true;
        }
        else
        {
            Falling = false;
        }

        ApplyAnimatorParams(speedXAbs, speedY, Jumping, Falling);

        if (Input.GetKeyDown(AttackKey) && canAttack)
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            animator.SetBool(paramisAttacking, isAttacking);

            if (Input.GetKey(KeyCode.UpArrow))
            {
                isUpAttacking = true;
                animator.SetBool(paramUpAttacking, isUpAttacking);
                animator.SetTrigger(paramUpAttack);
            }
            else
            {
                if (attackIndex == 0)
                {
                    animator.SetTrigger(paramAttack1);
                    attackIndex = 1;
                }
                else
                {
                    animator.SetTrigger(paramAttack2);
                    attackIndex = 0;
                }
            }
        }
    }


    private void ApplyAnimatorParams(float speedAbs, float verticalSpeed, bool Jumping, bool Falling)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(paramMoveSpeed, speedAbs);
        animator.SetBool(paramJumping, Jumping);
        animator.SetBool(paramFalling, Falling);
    }

    void OnAttackEnd()
    {
        isAttacking = false;
        animator.SetBool(paramisAttacking, isAttacking);
    }

    public bool GetIsUpAttacking()
    {
        return isUpAttacking;
    }

    void SetIsUpAttackingFalse()
    {
        isUpAttacking = false;
        animator.SetBool(paramUpAttacking, isUpAttacking);
    }
}
