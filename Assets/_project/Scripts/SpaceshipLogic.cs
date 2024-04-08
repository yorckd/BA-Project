using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceshipLogic : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;

    public HealthBar healthBar;

    void Start()
    {
        healthBar.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        
        if (currentHealth <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
