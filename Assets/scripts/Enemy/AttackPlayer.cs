using UnityEngine;

public class AttackPlayer : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer sr;

    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Transform SensorPoint;

    [SerializeField]
    private LayerMask groundMask;

    [SerializeField]
    private float detectRadius = 5f;

    [SerializeField]    
    private float fovAngle = 90f;

    [SerializeField]
    private PlayerHealth playerHealth;

    [SerializeField]
    private float moveSpeed = 7f;

    [SerializeField]
    private float attackInterval = 0.6f;

    private float attackTimer = 0f; 


    private void Start()
    {
        GameObject objPlayer = GameObject.Find("TheKnight");
        if (objPlayer != null)
        {
            player = objPlayer.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }


    private void FixedUpdate()
    {
        if(!CanSeePlayer())
        {
            return;
        }

       Vector2 direction = (player.position - transform.position).normalized;
        if(direction.x < 0)
        {
            sr.flipX = false;
        }
        else if(direction.x > 0)
        {
            sr.flipX = true;
        }

        attackTimer += Time.fixedDeltaTime;

        if(attackTimer >= attackInterval)
        {
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
            if (attackTimer >= 3f)
            {
                rb.linearVelocity = Vector2.zero;
                attackTimer = 0f;
            }
        }
    }


    bool CanSeePlayer()
    {
        if(player == null || SensorPoint == null)
        {
            return false;
        }

        bool isAlive = playerHealth.GetisAlive();

        Vector2 origin = SensorPoint.position;
        Vector2 toPlayer = (Vector2)(player.position - SensorPoint.position);
        float distance = toPlayer.magnitude;

        if(distance > detectRadius)
        {
            return false;
        }

        Vector2 forward = SensorPoint.right;
        float angle = Vector2.Angle(forward, toPlayer);

        if(angle > fovAngle * 0.5)
        {
            return false;
        }

        RaycastHit2D block = Physics2D.Raycast(origin, toPlayer.normalized, distance, groundMask);
        bool blocked = block.collider != null;

        if (blocked)
        {
            return false;
        }

        if (!isAlive)
        {
            return false;
        }

        return true;
    }

    
}
