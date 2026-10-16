using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerManager : MonoBehaviour
{
    //public InputActionReference InputActionReference; // another mean to work only with the "Action" from input assets
    public InputActionMap InputActionMap;
    public PlayerInput playerInput;
    // public InputActionReference ActionButton;
    FirstPersonController fpsCont;

    bool action = false;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        fpsCont = GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        var act = playerInput.actions["Action"];
        if(act.WasPressedThisFrame())
        {
            RaycastHit hitRobot;
            //Debug.Log($"WasPressedThisFrame {act} ");
            if (raycastWithTag("Robot", out hitRobot))
            {
                CallRobotConfig conf = hitRobot.transform.gameObject.GetComponent<CallRobotConfig>();
                if(conf != null) conf.CallMe(transform.TransformPoint(Vector3.right*1.5f));
                else
                {
                    conf = hitRobot.transform.gameObject.GetComponentInParent<CallRobotConfig>();
                    if (conf != null) conf.CallMe(transform.TransformPoint(Vector3.right * 1.5f));
                }
            }
        }
    }

    bool raycastWithTag(string tag, out RaycastHit hit)
    {
        hit = default(RaycastHit);
        if(Physics.Raycast(fpsCont.CinemachineCameraTarget.transform.position, fpsCont.CinemachineCameraTarget.transform.forward, out hit))
        {
            Debug.Log($"{hit.transform.name} - {hit.transform.tag}");
            if(hit.transform.tag == tag)
            {
                return true;
            }
        }
        return false;

    }

    void CallRobot()
    {

    }

    // Via Unity Events
    //public void OnAction(InputAction.CallbackContext context )
    //{
    //    Debug.Log($"{context}");
    //}
}
