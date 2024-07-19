using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkManagerHandler : MonoBehaviour
{
    public ModeManager modeManager;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void StartHost()
    {
        Debug.Log("Starting Host...");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully.");
            modeManager.setPilot(true);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void StartClient()
    {
        Debug.Log("Starting Client...");
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            modeManager.setPilot(false);
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count > 1)
        {
            Debug.Log("Client connected, loading game scene.");
            NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }
}