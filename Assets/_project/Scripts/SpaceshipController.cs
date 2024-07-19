using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceshipController : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public float moveSpeed = 5f;  // Speed of movement

    private Camera mainCamera;
    private float orthoSize;
    private float aspectRatio;

    private ReceiverUDP playerReceiver;
    private Keypoints playerKeypoints;

    private ModeManager modeManager;
    private bool isMotionControl;
    private bool isPilot;

    private float horizontalDirection;
    private float verticalDirection;
    private bool handsInFrontOfTorso = true;

    private float cameraWidth;
    private float cameraHeight;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    private void Start()
    {
        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();

        if (isPilot)
        {
            mainCamera = Camera.main;
            orthoSize = mainCamera.orthographicSize;
            aspectRatio = mainCamera.aspect;

            cameraWidth = orthoSize * 2f * aspectRatio;
            cameraHeight = orthoSize * 2f;

            minX = mainCamera.transform.position.x - cameraWidth / 2f;
            maxX = mainCamera.transform.position.x + cameraWidth / 2f;
            minY = mainCamera.transform.position.y - cameraHeight / 2f;
            maxY = mainCamera.transform.position.y + cameraHeight / 2f;


            if (isMotionControl)
            {
                playerReceiver = GameObject.FindWithTag("ReceiverSpaceship").GetComponent<ReceiverUDP>();
                //playerReceiver = GameObject.FindWithTag("ReceiverCrosshair").GetComponent<ReceiverTCP>();
                playerKeypoints = new Keypoints();

                StartCoroutine(UpdateDirections());
            }
        }
    }

    private void Update()
    {
        if (isPilot)
        {
            var newPos = transform.position;

            if (isMotionControl)
            {
                // YOLO controls
                newPos = transform.position +
                         new Vector3(horizontalDirection, verticalDirection, 0f) * moveSpeed * Time.deltaTime;
            }
            else
            {
                // Keyboard Controls
                float directionX = Input.GetAxis("Horizontal");
                float directionY = Input.GetAxis("Vertical");
                newPos = transform.position + new Vector3(directionX, directionY, 0f) * moveSpeed * Time.deltaTime;
            }

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            transform.position = newPos;
        }
    }

    private IEnumerator UpdateDirections()
    {
        while (true)
        {
            playerKeypoints.UpdateKeypoints(playerReceiver.GetNewestMessage());
            var newDirections = AngleToVector2(CalculateDirectionAngle());

            if (handsInFrontOfTorso)
            {
                newDirections = Vector2.zero;
            }

            verticalDirection = newDirections.x;
            horizontalDirection = newDirections.y;

            yield return null;
        }
    }

    private Vector2 AngleToVector2(float angle)
    {
        // Make sure a valid angle has been received
        if (float.IsNaN(angle))
        {
            return new Vector2(0f, 0f);
        }

        // Convert angle from degrees to radians
        float radians = angle * Mathf.Deg2Rad;

        // Calculate the direction vector
        float horizontalDirection = Mathf.Cos(radians);
        float verticalDirection = Mathf.Sin(radians) * -1;

        var directions = new Vector2(horizontalDirection, verticalDirection);
        return directions;
    }

    private float CalculateDirectionAngle()
    {
        var pointBetweenWrists = CalculatePointBetweenWrists();
        var bodyCenter = CalculateBodyCenter();
        var pivotAboveBodyCenter = new Vector2(bodyCenter.x, 1f);

        var verticalFromBodyCenter = pivotAboveBodyCenter - bodyCenter;
        var bodyCenterToLeftWrist = pointBetweenWrists - bodyCenter;

        // Calculate the angle using the dot product and magnitude
        float angle = Mathf.Atan2(bodyCenterToLeftWrist.y, bodyCenterToLeftWrist.x) - Mathf.Atan2(verticalFromBodyCenter.y, verticalFromBodyCenter.x);

        // Convert the angle from radians to degrees
        angle = angle * Mathf.Rad2Deg;

        // Ensure the angle is positive
        if (angle < 0f)
        {
            angle += 360f;
        }

        return angle;
    }

    private Vector2 CalculatePointBetweenWrists()
    {
        var leftWrist = playerKeypoints.keypoints[Bodyparts.L_WRIST];
        var rightWrist = playerKeypoints.keypoints[Bodyparts.R_WRIST];

        var pointBetweenWrists = new Vector2((leftWrist.x + rightWrist.x) / 2, (leftWrist.y + rightWrist.y) / 2);

        if (IsInFrontOfTorso(pointBetweenWrists))
        {
            handsInFrontOfTorso = true;
        }
        else
        {
            handsInFrontOfTorso = false;
        }

        return pointBetweenWrists;
    }

    private Vector2 CalculateBodyCenter()
    {
        var leftShoulder = playerKeypoints.keypoints[Bodyparts.L_SHOULDER];
        var rightShoulder = playerKeypoints.keypoints[Bodyparts.R_SHOULDER];
        var leftHip = playerKeypoints.keypoints[Bodyparts.L_HIP];
        var rightHip = playerKeypoints.keypoints[Bodyparts.R_HIP];

        // Line LeftShoulder to RightHip coefficients
        float a1 = rightHip.y - leftShoulder.y;
        float b1 = leftShoulder.x - rightHip.x;
        float c1 = a1 * leftShoulder.x + b1 * leftShoulder.y;

        // Line RightShoulder to LeftHip coefficients
        float a2 = leftHip.y - rightShoulder.y;
        float b2 = rightShoulder.x - leftHip.x;
        float c2 = a2 * rightShoulder.x + b2 * rightShoulder.y;

        float determinant = a1 * b2 - a2 * b1;

        if (Mathf.Approximately(determinant, 0))
        {
            // Lines are parallel
            return Vector2.negativeInfinity;
        }
        else
        {
            float xIntersection = (b2 * c1 - b1 * c2) / determinant;
            float yIntersection = (a1 * c2 - a2 * c1) / determinant;

            return new Vector2(xIntersection, yIntersection);
        }
    }

    private bool IsInFrontOfTorso(Vector2 point)
    {
        // This method checks whether the 'point between wrists' is in front of the players torso. If so the spaceship should stand still

        bool isOutside = false;

        var leftShoulder = playerKeypoints.keypoints[Bodyparts.L_SHOULDER];
        var rightHip = playerKeypoints.keypoints[Bodyparts.R_HIP];

        if (point.x > leftShoulder.x && point.x < rightHip.x)
        {
            if (point.y < leftShoulder.y && point.y > rightHip.y)
            {
                isOutside = true;
            }
        }

        return isOutside;
    }
}
