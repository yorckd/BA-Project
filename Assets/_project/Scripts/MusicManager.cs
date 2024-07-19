using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level1")
        {
            GetComponent<AudioSource>().Play();
        }

        if (scene.name == "GameOver")
        {
            GetComponent<AudioSource>().Stop();
        }
    }
}