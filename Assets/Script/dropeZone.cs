using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class dropeZone : MonoBehaviour
{
    public GameManager gameManager; // referencia al GameManager
    public float bonusTime = 5f;    // tiempo que suma cuando se suelta el objeto

    private void OnTriggerEnter(Collider other) {
        // Comprobar si el objeto tiene XRGrabInteractable
        if (other.GetComponent<XRGrabInteractable>() != null) {
            // Llamamos al GameManager para añadir tiempo
            gameManager.ObjectDroppedCorrectly(other.gameObject, bonusTime);
        }
    }
}
