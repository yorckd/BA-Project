using UnityEngine;
using Unity.Netcode;

public class EnemyBehavior : NetworkBehaviour
{
    private float movementSpeed;
    private int score;
    private int healthPoints;
    public GameObject deathAnimation;
    public AudioSource explosionSound;

    private ScoreManager scoreManager;

    private Vector3 movementDirection;
    private float timer;

    public void SetEnemyParameters(float moveSpeed, int enemyScore, int hp)
    {
        movementSpeed = moveSpeed;
        score = enemyScore;
        healthPoints = hp;
    }

    void Awake()
    {
        SetRandomDirection();
        timer = 5f;
    }

    void Start()
    {
        scoreManager = GameObject.FindWithTag("ScoreManager").GetComponent<ScoreManager>();
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position += movementDirection * movementSpeed * Time.deltaTime;
            transform.LookAt(Vector3.zero);

            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                SetRandomDirection();
                timer = Random.Range(3f, 7f);
            }

            StayInBounds();
        }
    }

    void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        movementDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
    }

    void StayInBounds()
    {
        Vector3 position = transform.position;
        Vector3 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, transform.position.z));

        if (position.x > screenBounds.x) movementDirection.x = -movementDirection.x;
        if (position.x < -screenBounds.x) movementDirection.x = -movementDirection.x;
        if (position.y > screenBounds.y) movementDirection.y = -movementDirection.y;
        if (position.y < -screenBounds.y) movementDirection.y = -movementDirection.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (other.CompareTag("friendlyMissile"))
            {
                healthPoints -= 1;

                if (healthPoints <= 0)
                {
                    Explode();
                    scoreManager.IncreseScoreServerRpc(score);
                }
            }
        }
    }

    void Explode()
    {
        if (deathAnimation != null)
        {
            GameObject explosion = Instantiate(deathAnimation, transform.position, transform.rotation);
            Destroy(explosion, 1f);
            PlayExplosionSound();
            ExplodeClientRpc();
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        GameObject explosion = Instantiate(deathAnimation, transform.position, transform.rotation);
        Destroy(explosion, 1f);
        PlayExplosionSound();
    }

    private void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            GameObject audioObject = new GameObject("CollisionSound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = explosionSound.clip;
            audioSource.volume = explosionSound.volume;
            audioSource.pitch = explosionSound.pitch;
            audioSource.spatialBlend = explosionSound.spatialBlend;
            audioSource.Play();
            Destroy(audioObject, explosionSound.clip.length);
        }
    }
}