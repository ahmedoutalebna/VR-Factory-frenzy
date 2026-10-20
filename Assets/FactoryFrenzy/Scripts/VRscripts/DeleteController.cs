using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeleteController : MonoBehaviour
{
    public InputActionReference deleteAction;
    public AudioSource sound;        // optional

    void OnEnable()
    {
        if (deleteAction == null) return;
        deleteAction.action.Enable();
        deleteAction.action.performed += OnDelete;
    }

    void OnDisable()
    {
        if (deleteAction == null) return;
        deleteAction.action.performed -= OnDelete;
    }

    void OnDelete(InputAction.CallbackContext ctx)
    {
        foreach (var item in new List<LevelItem>(LevelItem.HeldItems))
        {
            DeleteSound.Play(item.transform.position);
            Destroy(item.gameObject);
        }
    }
}