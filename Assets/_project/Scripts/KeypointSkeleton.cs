using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class KeypointSkeleton : NetworkBehaviour
{
    private ReceiverUDP playerReceiver;
    private Keypoints playerKeypoints;
    private List<NetworkObject> keypointObjects;

    public GameObject keypointPrefabPilot;
    public GameObject keypointPrefabGunner;

    private Camera mainCamera;
    private RectTransform canvasRectTransform;

    private ModeManager modeManager;
    private bool isMotionControl;
    private bool isPilot;

    void Start()
    {
        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();

        playerKeypoints = new Keypoints();
        keypointObjects = new List<NetworkObject>();

        if (isMotionControl)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                playerReceiver = GameObject.FindWithTag("ReceiverSpaceship").GetComponent<ReceiverUDP>();
            }
            else
            {
                playerReceiver = GameObject.FindWithTag("ReceiverCrosshair").GetComponent<ReceiverUDP>();
            }

            RequestSpawnKeypointVisualizersServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        mainCamera = Camera.main;

        if (isPilot)
        {
            canvasRectTransform = GameObject.FindWithTag("CanvasPilot").GetComponent<RectTransform>();
        }
        else
        {
            canvasRectTransform = GameObject.FindWithTag("CanvasGunner").GetComponent<RectTransform>();
        }

        StartCoroutine(UpdateKeypoints());
    }

    private IEnumerator UpdateKeypoints()
    {
        while (isMotionControl)
        {
            playerKeypoints.UpdateKeypoints(playerReceiver.GetNewestMessage());

            foreach (Bodyparts bodypart in Enum.GetValues(typeof(Bodyparts)))
            {
                Vector2 keypointPosition = new Vector2(playerKeypoints.keypoints[bodypart].x,
                    playerKeypoints.keypoints[bodypart].y);

                Vector3[] worldCorners = new Vector3[4];
                canvasRectTransform.GetWorldCorners(worldCorners);

                Vector3 bottomLeftCorner = worldCorners[0];
                Vector3 topRightCorner = worldCorners[2];

                float screenX = Mathf.Lerp(bottomLeftCorner.x, topRightCorner.x, keypointPosition.x);
                float screenY = Mathf.Lerp(bottomLeftCorner.y, topRightCorner.y, keypointPosition.y);

                Vector3 screenPosition = new Vector3(screenX, screenY, mainCamera.nearClipPlane);
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

                if (keypointObjects.Count > 0)
                {
                    var keypointObject = keypointObjects[GetEnumIndex(bodypart)];
                    if (keypointObject != null)
                    {
                        keypointObject.transform.position = worldPosition;

                        if (!IsHost)
                        {
                            UpdateKeypointPositionServerRpc(keypointObject.NetworkObjectId, worldPosition);
                        }
                    }
                }
            }

            yield return null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnKeypointVisualizersServerRpc(ulong clientId)
    {
        SpawnKeypointVisualizers(clientId);
    }

    private void SpawnKeypointVisualizers(ulong clientId)
    {
        Debug.Log("Called for client: " + clientId);
        foreach (Bodyparts bodypart in Enum.GetValues(typeof(Bodyparts)))
        {
            Vector3 keypointPosition = new Vector3(playerKeypoints.keypoints[bodypart].x, playerKeypoints.keypoints[bodypart].y, 0);
            GameObject keypointObject = new GameObject();

            if (clientId == 0)
            {
                keypointObject = Instantiate(keypointPrefabPilot, keypointPosition, Quaternion.identity);
            }
            else
            {
                keypointObject = Instantiate(keypointPrefabGunner, keypointPosition, Quaternion.identity);
            }

            NetworkObject networkObject = keypointObject.GetComponent<NetworkObject>();

            if (NetworkManager.Singleton.IsHost)
            {
                if (clientId == 0)
                {
                    networkObject.Spawn(true);
                    keypointObjects.Add(networkObject);
                }
                else
                {
                    networkObject.SpawnWithOwnership(clientId, true);
                    AddKeypointObjectClientRpc(networkObject.NetworkObjectId);
                }
            }
        }
    }

    [ClientRpc]
    private void AddKeypointObjectClientRpc(ulong networkObjectId)
    {
        AddKeypointObjects(networkObjectId);
    }

    private void AddKeypointObjects(ulong networkObjectId)
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                Debug.Log("adding");
                keypointObjects.Add(networkObject);
            }
            else
            {
                Debug.LogError($"NetworkObject with ID {networkObjectId} not found.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateKeypointPositionServerRpc(ulong networkObjectId, Vector3 newPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            networkObject.transform.position = newPosition;
        }
        else
        {
            Debug.LogError($"NetworkObject with ID {networkObjectId} not found.");
        }
    }

    public void DetermineRole()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            gameObject.tag = "SkeletonPilot";
        }
        else
        {
            gameObject.tag = "SkeletonGunner";
        }
    }

    private static int GetEnumIndex(Bodyparts bodypart)
    {
        return Array.IndexOf(Enum.GetValues(typeof(Bodyparts)), bodypart);
    }

    public Keypoints GetPlayerKeypoints()
    {
        return playerKeypoints;
    }
}
