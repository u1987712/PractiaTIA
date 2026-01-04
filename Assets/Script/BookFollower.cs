using UnityEngine;

public class BookFollower : MonoBehaviour
{
  
    [Header("Referencias")]
    public Transform head;           // La cámara del jugador
    public float distance = 0.6f;    // Distancia delante de la cabeza
    public float heightOffset = -0.25f; // Altura relativa a la cabeza
    public float smoothSpeed = 5f;   // Suavizado del movimiento
    public float fadeSpeed = 5f;     // Suavizado de aparición/desaparición

    [Header("Ángulo de visibilidad")]
    public float minAngle = -60f;  // mirar hacia abajo
    public float maxAngle = 30f;   // mirar hacia arriba

    [Header("inclinación extra")]
    public float tiltAngle = 10f;  // Inclinar la tapa ligeramente hacia ti

    private CanvasGroup canvasGroup; // Para fade opcional

    void Start()
    {
        // Añade CanvasGroup si no tiene (para controlar fade)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void LateUpdate()
    {
        // Posición: delante de la cabeza
        Vector3 targetPos = head.position + head.forward * distance + Vector3.up * heightOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);

        // Rotación: mirar hacia la cabeza correctamente
        Vector3 lookDir = head.position - transform.position;
        Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up);
        targetRot *= Quaternion.Euler(tiltAngle, 0f, 0f); // ligera inclinación
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);

        // Visibilidad: solo si la cabeza mira dentro del rango
        float pitch = head.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f; // convertir 0-360 a -180/180

        float targetAlpha = (pitch <= maxAngle && pitch >= minAngle) ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
}
