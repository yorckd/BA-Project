using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public Text scoreText;
    private int bonusScore = 0;
    private int score;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        score = (int)Time.time * 10 + bonusScore;
        scoreText.text = "Score: " + score.ToString();
    }

    public void IncreseScoreBy(int scoreIncrease)
    {
        bonusScore += scoreIncrease;
    }
}
