using UnityEngine;
using Unity.Netcode;

public class LaserBehavior : NetworkBehaviour
{
    private Vector3 target;
    private Vector3 direction;
    public GameObject collisionExplosion;
    public float speed;

    private bool isInitialized = false;

    void Update()
    {
        if (isInitialized)
        {
            transform.position += direction * speed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            }

            if (transform.position.z > 1000 || transform.position.z < -10)
            {
                if (IsHost)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    public void InitializeLaser(Vector3 newTarget)
    {
        target = newTarget;
        direction = (target - transform.position).normalized;
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsHost)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Asteroid") || other.CompareTag("HealthKit"))
            {
                Explode();
            }

            if (other.CompareTag("Spaceship"))
            {
                SpaceshipLogic spaceship = other.GetComponent<SpaceshipLogic>();
                spaceship.TakeDamage(1);
                Explode();
            }
        }
    }

    void Explode()
    {
        if (collisionExplosion != null)
        {
            GameObject explosion = Instantiate(collisionExplosion, transform.position, transform.rotation);
            ExplodeClientRpc();
            Destroy(gameObject);
            Destroy(explosion, 1f);
        }
    }

    [ClientRpc]
    private void ExplodeClientRpc()
    {
        GameObject explosion = Instantiate(collisionExplosion, transform.position, transform.rotation);
        Destroy(explosion, 1f);
    }
}