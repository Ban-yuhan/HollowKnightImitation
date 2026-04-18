using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int maxHealth = 10;

    private bool isAlive;

    [SerializeField]
    private int currentHealth;

    [SerializeField]
    private PlayerSkill skill;

    private void Start()
    {
        currentHealth = maxHealth;
        isAlive = true;
    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (skill.isCryDashing)
        {
            skill.isCryDashing = false;
        }

        if (currentHealth <= 0)
        {
            isAlive = false;
            Die();
        }
    }


    void Die()
    {
        Destroy(gameObject, 1f);
        
    }


    public bool GetisAlive()
    {
        return isAlive;
    }
}
