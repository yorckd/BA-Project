using System;
using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class PoseRecognizer : NetworkBehaviour
{
    public Text currentPosePilot;
    public Text repCountPilot;
    private NetworkVariable<int> repCountPilotNetwork = new NetworkVariable<int>();
    public Text currentPoseGunner;
    public Text repCountGunner;
    private NetworkVariable<int> repCountGunnerNetwork = new NetworkVariable<int>();
    public Text instructions;
    private KeypointSkeleton playerSkeleton;

    public int necessaryReps;
    private bool playerIsStanding;
    private NetworkVariable<bool> exerciseCompletedPilot = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> exerciseCompletedGunner = new NetworkVariable<bool>(false);
    private bool exitingFlag = false;

    private ModeManager modeManager;
    private bool isMotionControl;
    private bool isPilot;

    void Start()
    {
        var skeletonWithoutRole = GameObject.FindWithTag("SkeletonRaw").GetComponent<KeypointSkeleton>();
        skeletonWithoutRole.DetermineRole();

        modeManager = GameObject.FindWithTag("ModeManager").GetComponent<ModeManager>();
        isMotionControl = modeManager.IsMotionMode();
        isPilot = modeManager.IsPilot();
        
        instructions.text = $"Do {necessaryReps} squats!";
        playerIsStanding = false;

        if (isMotionControl)
        {
            if (isPilot)
            {
                playerSkeleton = GameObject.FindWithTag("SkeletonPilot").GetComponent<KeypointSkeleton>();
                if (playerSkeleton == null) { Debug.Log("couldn't find pilot skeleton"); }
            }
            else
            {
                playerSkeleton = GameObject.FindWithTag("SkeletonGunner").GetComponent<KeypointSkeleton>();
                if (playerSkeleton == null) { Debug.Log("couldn't find gunner skeleton"); }
            }
        }
        else
        {
            if (isPilot)
            {
                exerciseCompletedPilot.Value = true;
            }
            else
            {
                UpdateExerciseStatusOfGunnerServerRpc(true);
            }
        }
    }

    void Update()
    {
        if (isMotionControl)
        {
            if (isPilot)
            {
                currentPosePilot.text = identifyLegPose();

                if (repCountPilotNetwork.Value >= necessaryReps)
                {
                    exerciseCompletedPilot.Value = true;
                }
            }
            else
            {
                currentPoseGunner.text = identifyLegPose();

                if (repCountGunnerNetwork.Value >= necessaryReps)

                {
                    UpdateExerciseStatusOfGunnerServerRpc(true);
                }
            }
        }

        repCountPilot.text = repCountPilotNetwork.Value.ToString();
        repCountGunner.text = repCountGunnerNetwork.Value.ToString();

        if (exerciseCompletedPilot.Value && exerciseCompletedGunner.Value && !exitingFlag)
        {
            exitingFlag = true;
            ExitExerciseStage();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateExerciseStatusOfGunnerServerRpc(bool newValue)
    {
        exerciseCompletedGunner.Value = newValue;
    }

    [ClientRpc]
    private void UpdateInstructionsClientRpc(string newInstruction)
    {
        instructions.text = newInstruction;
    }

    private string identifyLegPose()
    {
        var legPose = " ";

        var kneeAngle = playerSkeleton.GetPlayerKeypoints().AngleBetweenPoints(Bodyparts.L_HIP, Bodyparts.L_KNEE, Bodyparts.L_ANKLE);

        if (kneeAngle > 30f && kneeAngle < 110f)
        {
            legPose = "squatting";
            if (playerIsStanding)
            {
                playerIsStanding = false;
                if (isPilot)
                {
                    repCountPilotNetwork.Value += 1;
                    Debug.Log("Increasing");
                }
                else
                {
                    IncreaseGunnerRepCountServerRpc();
                }
            }
        }
        else if (kneeAngle < 10f || kneeAngle > 160f)
        {
            legPose = "standing";
            playerIsStanding = true;
        }

        return legPose;
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseGunnerRepCountServerRpc()
    {
        repCountGunnerNetwork.Value += 1;
    }

    private void ExitExerciseStage()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            StartCoroutine(ExitExerciseStageCoroutine());
        }
    }

    private IEnumerator ExitExerciseStageCoroutine()
    {
        if (SceneManager.GetActiveScene().name == "ExerciseStage1")
        {
            instructions.text = "Exercise completed!";
            UpdateInstructionsClientRpc(instructions.text);
            yield return new WaitForSeconds(5f);

            instructions.text = "Get ready for Level 2!";
            UpdateInstructionsClientRpc(instructions.text);
            yield return new WaitForSeconds(3f);

            NetworkManager.Singleton.SceneManager.LoadScene("Level2", LoadSceneMode.Single);
        }
        else
        {
            instructions.text = "Exercise completed!";
            UpdateInstructionsClientRpc(instructions.text);
            yield return new WaitForSeconds(5f);

            NetworkManager.Singleton.SceneManager.LoadScene("GameEnd", LoadSceneMode.Single);
        }
    }
}
