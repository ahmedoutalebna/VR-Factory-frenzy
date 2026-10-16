using UnityEngine;
using UnityEngine.InputSystem; // Indispensable pour le nouvel Input System

public class CameraSwitcher : MonoBehaviour
{
    [Tooltip("Glisse la FlyCam ici")]
    [SerializeField] private GameObject flyCam;

    void Update()
    {
        // Si on appuie sur la touche 'C' du clavier
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (flyCam != null)
            {
                // Inverse l'état d'activation de la FlyCam (On/Off)
                flyCam.SetActive(!flyCam.activeSelf);
            }
        }
    }
}