using UnityEngine;
using Unity.Netcode;

public class AsteroidBehavior : NetworkBehaviour
{
    private Vector3 target;
    private float speed;
    private int score;
    private float rotationSpeed;
    private Vector3 rotationDirection;
    public AudioSource collisionSound;
    private ScoreManager scoreManager;

    public void SetMovementParameters(Vector3 targetTransform, float moveSpeed, int asteroidScore)
    {
        scoreManager = GameObject.FindWithTag("ScoreManager").GetComponent<ScoreManager>();

        target = targetTransform;
        speed = moveSpeed;
        score = asteroidScore;
        rotationSpeed = Random.Range(10f, 50f);
        rotationDirection = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), Random.Range(-3, 3));
    }

    private void Update()
    {
        if (IsServer)
        {
            if (target == null) return;

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            transform.Rotate(rotationDirection * (rotationSpeed * Time.deltaTime));

            if (transform.position.z == target.z)
            {
                Destroy(gameObject);
                scoreManager.IncreseScoreServerRpc(score);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHost)
        {
            if (other.CompareTag("Spaceship"))
            {
                SpaceshipLogic spaceshipLogic = other.GetComponent<SpaceshipLogic>();
                if (spaceshipLogic != null)
                {
                    spaceshipLogic.TakeDamage(1);

                    PlayCollisionSound();
                    PlayCollisionSoundClientRpc();
                }

                Destroy(gameObject);
            }

            if (other.CompareTag("AsteroidAimableBarrier"))
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }
    }

    [ClientRpc]
    void PlayCollisionSoundClientRpc()
    {
        if (!IsHost)
        {
            PlayCollisionSound();
        }
    }

    void PlayCollisionSound()
    {
        if (collisionSound != null)
        {
            GameObject audioObject = new GameObject("CollisionSound");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = collisionSound.clip;
            audioSource.volume = collisionSound.volume;
            audioSource.pitch = collisionSound.pitch;
            audioSource.spatialBlend = collisionSound.spatialBlend;
            audioSource.Play();
            Destroy(audioObject, collisionSound.clip.length);
        }
    }
}