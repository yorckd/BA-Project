using Unity.Netcode;
using UnityEngine;

public class EscMenuManager : NetworkBehaviour
{
    public static bool isPaused = false;

    public GameObject pauseMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsHost)
            {
                if (!isPaused)
                {
                    PauseGameServerRpc();
                }
            }
            else if (IsClient)
            {
                if (!isPaused)
                {
                    RequestPauseGameServerRpc();
                }
            }
        }
    }

    private void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    private void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (IsHost)
        {
            ResumeGameServerRpc();
        }
        else
        {
            RequestResumeGameServerRpc();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc()
    {
        Pause();
        PauseGameClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResumeGameServerRpc()
    {
        Resume();
        ResumeGameClientRpc();
    }

    [ClientRpc]
    private void PauseGameClientRpc()
    {
        if (!IsHost)
        {
            Pause();
        }
    }

    [ClientRpc]
    private void ResumeGameClientRpc()
    {
        if (!IsHost)
        {
            Resume();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPauseGameServerRpc(ServerRpcParams rpcParams = default)
    {
        PauseGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestResumeGameServerRpc(ServerRpcParams rpcParams = default)
    {
        ResumeGameServerRpc();
    }
}