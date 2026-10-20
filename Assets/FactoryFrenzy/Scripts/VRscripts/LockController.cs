using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockController : MonoBehaviour
{
    public InputActionReference lockAction;

    void OnEnable()
    {
        if (lockAction == null) return;
        lockAction.action.Enable();
        lockAction.action.performed += OnLock;
    }

    void OnDisable()
    {
        if (lockAction == null) return;
        lockAction.action.performed -= OnLock;
    }

    void OnLock(InputAction.CallbackContext ctx)
    {
        if (LevelItem.Current != null)
            LevelItem.Current.ToggleLock();
    }
}