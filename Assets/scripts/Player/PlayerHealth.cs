using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 10;

    private bool isAlive;

    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;

    }
}
