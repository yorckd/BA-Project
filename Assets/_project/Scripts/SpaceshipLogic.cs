using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceshipLogic : NetworkBehaviour
{
    public int maxHealth;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(0);

    HealthBar healthBar;

    private void Start()
    {
        healthBar = GameObject.Find("HealthBar").GetComponent<HealthBar>();
        if (IsHost)
        {
            healthBar.SetMaxHealth(maxHealth);
            currentHealth.Value = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsHost)
        {
            currentHealth.Value -= damage;

            healthBar.SetHealth(currentHealth.Value);
            UpdateScoreClientRpc(currentHealth.Value);

            if (currentHealth.Value <= 0)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("GameOver", LoadSceneMode.Single); ;
            }
        }
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int value)
    {
        healthBar.SetHealth(value);
    }

    public bool IsDamaged()
    {
        if (currentHealth.Value < maxHealth)
        {
            return true;
        }

        return false;
    }
}
