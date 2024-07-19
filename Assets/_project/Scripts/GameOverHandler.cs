using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverHandler : NetworkBehaviour
{
    public void RestartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
