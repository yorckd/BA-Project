using Unity.Netcode;
using UnityEngine;

public class LaserCannon : NetworkBehaviour
{
    public float fireRateKeyboard;
    public float fireRateMotion;
    private float fireRateTimeStamp;

    public GameObject projectilePrefab;
    private GameObject crosshair;
    public AudioSource shootingSound;

    private ModeManager modeManager;
    private bool isMotionControl;
    private bool isPilot;

    void Start()
    {
        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();

        fireRateTimeStamp = Time.time + 1f;
    }

    void Update()
    {
        if (crosshair == null)
        {
            crosshair = GameObject.FindWithTag("Crosshair") ;
        }

        if (!isPilot)
        {
            if (isMotionControl)
            {
                if (Time.time > fireRateTimeStamp)
                {
                    RequestShootLaserServerRpc();
                    fireRateTimeStamp = Time.time + fireRateMotion;
                }
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    if (Time.time > fireRateTimeStamp)
                    {
                        RequestShootLaserServerRpc();
                        fireRateTimeStamp = Time.time + fireRateKeyboard;
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestShootLaserServerRpc()
    {
        ShootLaser();
    }

    private void ShootLaser()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            GameObject laserObject = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            NetworkObject networkObject = laserObject.GetComponent<NetworkObject>();
            networkObject.Spawn(true);
            var destination = crosshair.transform.position;
            laserObject.GetComponent<LaserBehavior>().InitializeLaser(destination);
            shootingSound.Play();

            ShootLaserClientRpc(networkObject.NetworkObjectId, destination);
        }
    }

    [ClientRpc]
    void ShootLaserClientRpc(ulong laserNetworkObjectId, Vector3 target)
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