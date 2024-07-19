using UnityEngine;
using Unity.Netcode;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemySpaceshipPrefab;
    public float moveSpeed;
    public int enemySpaceshipScore;
    public int healthPoints;

    private bool isEnemyActive;
    private float nextSpawnTime;

    private void Start()
    {
        isEnemyActive = false;
        nextSpawnTime = Time.time + 5f;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnPrefab();
                nextSpawnTime = float.MaxValue;
            }

            if (GameObject.FindGameObjectWithTag("Enemy"))
            {
                isEnemyActive = false;
            }

            GenerateNextSpawnTime();
        }
    }

    private void SpawnPrefab()
    {
        Vector3 spawnPosition = new(Random.Range(-20f, 20f), Random.Range(-10f, 10f), 100f);
        GameObject newEnemy = Instantiate(enemySpaceshipPrefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = newEnemy.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }

        EnemyBehavior prefabMovement = newEnemy.GetComponent<EnemyBehavior>();
        if (prefabMovement != null)
        {
            prefabMovement.SetEnemyParameters(moveSpeed, enemySpaceshipScore, healthPoints);
        }
    }

    private void GenerateNextSpawnTime()
    {
        if (!isEnemyActive)
        {
            nextSpawnTime = Time.time + Random.Range(3f, 7f);
            isEnemyActive = true;
        }
    }
}