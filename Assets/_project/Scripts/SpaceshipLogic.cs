using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipLogic : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    public HealthBar healthBar;

    void Start()
    {
        healthBar.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
    }

    void Update()
    {
        
    }
}
