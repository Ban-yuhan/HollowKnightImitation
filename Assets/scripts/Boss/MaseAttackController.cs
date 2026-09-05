using UnityEngine;

public class MaseAttackController : MonoBehaviour
{
    [SerializeField] private GameObject JumpAttack;

    private bool isJumpAttackActive;
    private float JumpAttackTimer;
    private float JumpAttackDuration = 0.2f;


    private void Start()
    {
        isJumpAttackActive = false;
        JumpAttackTimer = 0;
    }


    private void Update()
    {
        if (isJumpAttackActive)
        {
            JumpAttackTimer += Time.deltaTime;

            if (JumpAttackTimer >= JumpAttackDuration)
            {
                InActiveJumpAttackHitBox();
            }
        }
    }


    public void ActiveJumpAttackHitBox()
    {
        JumpAttack.SetActive(true);
        isJumpAttackActive = true;
    }

    private void InActiveJumpAttackHitBox()
    {
        JumpAttack.SetActive(false);
        isJumpAttackActive = false;
    }
}

