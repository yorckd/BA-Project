using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;
    public float minSpawnInterval;
    public float maxSpawnInterval;
    public float moveSpeed;
    public int asteroidScore;

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = GetNextSpawnTime();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnPrefab();
            nextSpawnTime = GetNextSpawnTime();
        }
    }

    private void SpawnPrefab()
    {
        Vector3 spawnPosition = new(Random.Range(-20f, 20f), Random.Range(-10f, 10f), 100f);
        Vector3 targetPosition = new(spawnPosition.x, spawnPosition.y, -20f);
        GameObject newAsteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);

        AsteroidBehavior prefabMovement = newAsteroid.GetComponent<AsteroidBehavior>();
        if (prefabMovement != null)
        {
            prefabMovement.SetMovementParameters(targetPosition, moveSpeed, asteroidScore);
        }
    }

    private float GetNextSpawnTime()
    {
        return Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}
