using UnityEngine;
using Unity.Netcode;

public class EnemyShooting : NetworkBehaviour
{
    public float fireRate;
    private float fireRateTimeStamp;

    public GameObject projectilePrefab;
    private GameObject spaceship;
    public AudioSource shootingSound;

    void Start()
    {
        spaceship = GameObject.FindGameObjectWithTag("Spaceship");
        fireRateTimeStamp = Time.time + fireRate;
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (Time.time > fireRateTimeStamp)
            {
                shootLaser();
                fireRateTimeStamp = Time.time + fireRate;
            }
        }
    }

    void shootLaser()
    {
        Vector3 laserSpawnPosition = transform.position;
        Quaternion laserSpawnRotation = transform.rotation;

        GameObject laser = Instantiate(projectilePrefab, laserSpawnPosition, laserSpawnRotation);
        var laserDestination = spaceship.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

        NetworkObject networkObject = laser.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
            laser.GetComponent<LaserBehavior>().InitializeLaser(laserDestination);
            shootingSound.Play();

            FireLaserClientRpc(laser.GetComponent<NetworkObject>().NetworkObjectId, laserDestination);
        }
    }

    [ClientRpc]
    void FireLaserClientRpc(ulong laserNetworkObjectId, Vector3 target)
    {
        if (!IsHost)
        {
            NetworkObject laserNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[laserNetworkObjectId];
            GameObject laser = laserNetworkObject.gameObject;
            laser.GetComponent<LaserBehavior>().InitializeLaser(target);
            shootingSound.Play();
        }
    }
}