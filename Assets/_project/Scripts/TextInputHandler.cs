using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;

public class TextInputHandler : MonoBehaviour
{
    public GameObject networkManagerObject; // Reference to NetworkManager GameObject
    private NetworkManager networkManager;

    private string hostIp = "0.0.0.0";
    private ushort gamePort = 8888;

    void Start()
    {
        if (networkManagerObject != null)
        {
            networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (networkManager != null)
            {
                var transport = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
                if (transport != null)
                {
                    transport.SetConnectionData(hostIp, gamePort);
                    Debug.Log($"Server IP: {hostIp}");
                    Debug.Log($"Server Port: {gamePort}");
                }
                else
                {
                    Debug.LogError("UnityTransport component not found on NetworkManager.");
                }
            }
        }
    }

    public void UpdateHostIp(string ip)
    {
        hostIp = ip;
        var transport = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport != null)
        {
            transport.SetConnectionData(ip, gamePort);
            Debug.Log($"Updated Server IP: {hostIp}");
        }
        else
        {
            Debug.LogError("UnityTransport component not found on NetworkManager.");
        }
    }

    public void UpdateGamePort(string newPort)
    {
        if (TryConvertToUShort(newPort, out ushort port))
        {
            gamePort = port;
            var transport = networkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (transport != null)
            {
                transport.SetConnectionData(hostIp, port);
                Debug.Log($"Updated Server Port: {gamePort}");
            }
            else
            {
                Debug.LogError("UnityTransport component not found on NetworkManager.");
            }
        }
        else
        {
            Debug.LogError("Conversion failed. The provided string is not a valid ushort.");
        }
    }

    bool TryConvertToUShort(string input, out ushort result)
    {
        return ushort.TryParse(input, out result);
    }
}
