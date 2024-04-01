using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed of movement

    private Camera mainCamera;
    private float orthoSize;
    private float aspectRatio;

    void Start()
    {
        mainCamera = Camera.main;
        orthoSize = mainCamera.orthographicSize;
        aspectRatio = mainCamera.aspect;
    }

    void Update()
    {
        float cameraWidth = orthoSize * 2f * aspectRatio;
        float cameraHeight = orthoSize * 2f;

        float minX = mainCamera.transform.position.x - cameraWidth / 2f;
        float maxX = mainCamera.transform.position.x + cameraWidth / 2f;
        float minY = mainCamera.transform.position.y - cameraHeight / 2f;
        float maxY = mainCamera.transform.position.y + cameraHeight / 2f;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 newPos = transform.position + new Vector3(moveX, moveY, 0f) * moveSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        transform.position = newPos;
    }
}
