using Unity.Netcode;
using UnityEngine;

public class AsteroidSpawner : NetworkBehaviour
{
    public GameObject asteroidPrefab;
    public float minSpawnInterval;
    public float maxSpawnInterval;
    public float moveSpeed;
    public int asteroidScore;

    private float nextSpawnTime;

    private void Start()
    {
        GenerateNextSpawnTime();
    }

    private void Update()
    {
        if (IsServer && Time.time >= nextSpawnTime)
        {
            SpawnPrefab();
            GenerateNextSpawnTime();
        }
    }

    private void SpawnPrefab()
    {
        Vector3 spawnPosition = new(Random.Range(-20f, 20f), Random.Range(-10f, 10f), 100f);
        Vector3 targetPosition = new(spawnPosition.x, spawnPosition.y, -20f);
        GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = newAsteroid.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }

        AsteroidBehavior prefabMovement = newAsteroid.GetComponent<AsteroidBehavior>();
        if (prefabMovement != null)
        {
            prefabMovement.SetMovementParameters(targetPosition, moveSpeed, asteroidScore);
        }
    }

    private void GenerateNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
