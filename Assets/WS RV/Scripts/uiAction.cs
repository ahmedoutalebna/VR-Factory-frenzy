using com.lineact.lit.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class UIAction : MonoBehaviour
{
    [SerializeField] GameObject tiago;
    [SerializeField] Transform xrRig;
    [SerializeField] State patrolState;
    [SerializeField] XRRayInteractor leftRayInteractor, rightRayInteractor;
    [SerializeField] InputActionReference activateLeft, activateRight;
    [SerializeField] Transform head;   // Main Camera inside the XR Origin

    [Tooltip("Distance (m) in front of the player where the robot stops")]
    [SerializeField] float callDistance = 1f;

    [Tooltip("Max distance (m) to search for a valid NavMesh point")]
    [SerializeField] float navMeshSnapRadius = 3f;

    public void Calling()
    {
        if (RobotFireTrigger.RobotBroken) return;

        var callRobotConfig = tiago.GetComponent<CallRobotConfig>();

        // Player's head position, flattened to the floor
        Vector3 playerPos = head.position;
        playerPos.y = xrRig.position.y;

        // Stop in front of the player so the robot doesn't try to enter them
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        Vector3 target = playerPos + forward * callDistance;

        target = SnapToNavMesh(target);

        Debug.Log($"Calling -> head: {head.position} | rig: {xrRig.position} | target: {target}");
        callRobotConfig?.CallMe(target);
    }

    public void Patrol()
    {
        if (RobotFireTrigger.RobotBroken) return;

        var stateMachine = tiago.GetComponent<BaseStateMachine>();
        stateMachine?.ChangeState(patrolState);
    }

    /// <summary>
    /// Method to call when you want to specify the position where the robot must go to
    /// </summary>
    public void GoToInit()
    {
        if (RobotFireTrigger.RobotBroken) return;

        activateLeft.action.started -= Action_started;
        activateRight.action.started -= Action_started;

        activateLeft.action.started += Action_started;
        activateRight.action.started += Action_started;

        // the next trigger will get the position of the ray interactor and ask the robot to move to.
    }

    private void Action_started(InputAction.CallbackContext obj)
    {
        activateLeft.action.started -= Action_started;
        activateRight.action.started -= Action_started;

        // The robot may have broken while we were waiting for the click
        if (RobotFireTrigger.RobotBroken) return;

        // Use the ray of the hand that pressed the trigger
        XRRayInteractor ray = (obj.action == activateLeft.action) ? leftRayInteractor : rightRayInteractor;

        Vector3 pos, normal;
        if (ray.TryGetHitInfo(out pos, out normal, out _, out _))
        {
            Debug.Log($"At {pos}");
            GoToPosition(pos);
        }
    }

    public void GoToPosition(Vector3 position)
    {
        if (RobotFireTrigger.RobotBroken) return;

        var callRobotConfig = tiago.GetComponent<CallRobotConfig>();
        callRobotConfig?.CallMe(SnapToNavMesh(position));
    }

    /// <summary>
    /// Returns the closest valid NavMesh point, or the original point if none is found.
    /// </summary>
    private Vector3 SnapToNavMesh(Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSnapRadius, NavMesh.AllAreas))
            return hit.position;

        Debug.LogWarning($"No NavMesh point found near {position}");
        return position;
    }
}