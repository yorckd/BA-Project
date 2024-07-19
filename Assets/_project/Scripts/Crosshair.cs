using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Crosshair : NetworkBehaviour
{
    private GameObject spaceship;
    private int layerToHit;

    private ReceiverUDP playerReceiver;
    private Keypoints playerKeypoints;

    private ModeManager modeManager;
    private bool isMotionControl;
    private bool isPilot;

    float screenWidth = Screen.width;
    float screenHeight = Screen.height;

    void Start()
    {

        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();
        layerToHit = LayerMask.GetMask("crosshair aimable");

        if (!isPilot)
        {
            if (isMotionControl)
            {
                GameObject receiverObject = GameObject.FindGameObjectWithTag("ReceiverCrosshair");
                if (receiverObject != null)
                {
                    playerReceiver = receiverObject.GetComponent<ReceiverUDP>();
                }
                else
                {
                    Debug.Log("Found no receiver");
                }

                playerKeypoints = new Keypoints();

                StartCoroutine(UpdateDirections());
            }
        }
    }

    void Update()
    {
        if (!isPilot)
        {
            var screenPosition = Vector3.zero;

            if (isMotionControl)
            {
                var handsPosition = CalculatePointBetweenWrists();
                screenPosition = new Vector3(handsPosition.x * screenWidth, handsPosition.y * screenHeight, 0);
            }
            else
            {
                screenPosition = Input.mousePosition;
            }

            float maxAimDistance = 1000f;
            var crosshairDirection = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, maxAimDistance));

            if (Physics.Raycast(Camera.main.transform.position, crosshairDirection, out RaycastHit raycastHit,
                    Mathf.Infinity, layerToHit))
            {
                Vector3 newPosition = new Vector3(raycastHit.point.x, raycastHit.point.y, raycastHit.point.z - 3f);
                transform.position = newPosition;
                UpdateCrosshairPositionServerRpc(newPosition);
            }
        }
    }

    private IEnumerator UpdateDirections()
    {
        while (true)
        {
            playerKeypoints.UpdateKeypoints(playerReceiver.GetNewestMessage());

            yield return null;
        }
    }

    private Vector2 CalculatePointBetweenWrists()
    {
        var leftWrist = playerKeypoints.keypoints[Bodyparts.L_WRIST];
        var rightWrist = playerKeypoints.keypoints[Bodyparts.R_WRIST];

        var pointBetweenWrists = new Vector2((leftWrist.x + rightWrist.x) / 2, (leftWrist.y + rightWrist.y) / 2);

        return pointBetweenWrists;
    }

    [ServerRpc]
    private void UpdateCrosshairPositionServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
