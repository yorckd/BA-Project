using Unity.Netcode;
using UnityEngine;

public class PlayerObjectSpawner : NetworkBehaviour
{
    public GameObject crosshairPrefab;
    public GameObject spaceshipPrefab;
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (NetworkManager.Singleton.IsHost)
        {
            while (NetworkManager.Singleton.ConnectedClients.Count < 2)
            {
                Debug.Log("Waiting for client");
            }

            SpawnCrosshairWithClientOwnershipServerRpc(1);
        }
    }

    private void SpawnSpaceship()
    {
        if (networkManager.IsHost)
        {
            GameObject spaceship = Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject networkObject = spaceship.GetComponent<NetworkObject>();

            networkObject.Spawn(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCrosshairWithClientOwnershipServerRpc(ulong clientId)
    {
        if (networkManager.IsHost)
        {
            GameObject crosshair = Instantiate(crosshairPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject networkObject = crosshair.GetComponent<NetworkObject>();

            networkObject.SpawnWithOwnership(clientId, true);
        }
    }
}
