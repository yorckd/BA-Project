using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealthKitSpawner : NetworkBehaviour
{
    public GameObject healthKitPrefab;
    public float moveSpeed;
    public int bonusScore;
    public int healthPoints;

    private bool isHealthKitActive;
    private float nextSpawnTime;

    private void Start()
    {
        isHealthKitActive = false;
        nextSpawnTime = Time.time + 15f;
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

            if (GameObject.FindGameObjectWithTag("HealthKit"))
            {
                isHealthKitActive = false;
            }

            GenerateNextSpawnTime();
        }
    }

    private void SpawnPrefab()
    {
        Vector3 spawnPosition = new(Random.Range(-20f, 20f), Random.Range(-10f, 10f), 70f);
        GameObject newHealthKit = Instantiate(healthKitPrefab, spawnPosition, Quaternion.Euler(new Vector3(270, 0, 0)));

        NetworkObject networkObject = newHealthKit.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }

        HealthKitBehavior prefabBehavior = newHealthKit.GetComponent<HealthKitBehavior>();
        if (prefabBehavior != null)
        {
            prefabBehavior.SetHealthKitParameters(moveSpeed, bonusScore, healthPoints);
        }
    }

    private void GenerateNextSpawnTime()
    {
        if (!isHealthKitActive)
        {
            nextSpawnTime = Time.time + Random.Range(10f, 20f);
            isHealthKitActive = true;
        }
    }
}
