using UnityEngine;

public class AsteroidBehavior : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private int score;

    public void SetMovementParameters(Vector3 targetTransform, float moveSpeed, int asteroidScore)
    {
        target = targetTransform;
        speed = moveSpeed;
        score = asteroidScore;
    }

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position.z == target.z)
        {
            Destroy(gameObject);
            ScoreManager.instance.IncreseScoreBy(score);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Spaceship"))
        {
            Destroy(gameObject);
        }
    }
}