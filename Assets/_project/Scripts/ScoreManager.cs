using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager instance;

    public int necessaryScore;

    public Text scoreText;
    private int bonusScore = 0;
    private float startTime;
    private NetworkVariable<int> score = new NetworkVariable<int>(0);

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (IsHost)
        {
            score.Value = (int)(Time.time - startTime) * 10 + bonusScore;
            scoreText.text = "Score: " + score.Value.ToString();
            UpdateScoreClientRpc();

            if (score.Value >= necessaryScore)
            {
                if (SceneManager.GetActiveScene().name == "Level1")
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("ExerciseStage1", LoadSceneMode.Single);
                }
                else
                {
                    NetworkManager.Singleton.SceneManager.LoadScene("ExerciseStage2", LoadSceneMode.Single);
                }
            }
        }
    }

    [ServerRpc]
    public void IncreseScoreServerRpc(int scoreIncrease)
    {
        bonusScore += scoreIncrease;
    }

    [ClientRpc]
    private void UpdateScoreClientRpc()
    {
        // Update the UI with the new score
        scoreText.text = "Score: " + score.Value.ToString();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level1")
        {
            startTime = Time.time;
        }

        if (scene.name == "Level2")
        {
            startTime = Time.time;
        }
    }
}
