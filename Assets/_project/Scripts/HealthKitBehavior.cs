using UnityEngine;
using Unity.Netcode;

public class HealthKitBehavior : NetworkBehaviour
{
    private float movementSpeed;
    private int score;
    private int healthPoints;
    private ScoreManager scoreManager;
    private SpaceshipLogic spaceshipLogic;

    private Vector3 movementDirection;
    private float timer;

    public void SetHealthKitParameters(float moveSpeed, int bonusScore, int hp)
    {
        movementSpeed = moveSpeed;
        score = bonusScore;
        healthPoints = hp;
    }

    void Awake()
    {
        SetRandomDirection();
        timer = 3f;
    }

    void Start()
    {
        scoreManager = GameObject.FindWithTag("ScoreManager").GetComponent<ScoreManager>();
        spaceshipLogic = GameObject.FindWithTag("Spaceship").GetComponent<SpaceshipLogic>();
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position += movementDirection * movementSpeed * Time.deltaTime;

            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                SetRandomDirection();
                timer = Random.Range(3f, 5f);
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
                    if (spaceshipLogic.IsDamaged())
                    {
                        spaceshipLogic.TakeDamage(-1);
                    }
                    else
                    {
                        scoreManager.IncreseScoreServerRpc(score);
                    }

                    Explode();
                }
            }
        }
    }

    void Explode()
    {
        Destroy(gameObject);
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        
    }
}
