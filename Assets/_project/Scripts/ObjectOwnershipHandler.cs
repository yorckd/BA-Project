using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class ObjectOwnershipHandler : NetworkBehaviour
{
    public NetworkObject crosshair;

    void Awake()
    {
        Debug.Log(NetworkManager.Singleton.IsHost);
        if (NetworkManager.Singleton.IsHost)
        {
            var clientId = NetworkManager.Singleton.ConnectedClientsIds.FirstOrDefault();
            Debug.Log(NetworkManager.Singleton.ConnectedClientsIds.ToString());

            Debug.Log(crosshair.OwnerClientId);
            crosshair.ChangeOwnership(clientId);
            Debug.Log(crosshair.OwnerClientId);
        }
    }
}
