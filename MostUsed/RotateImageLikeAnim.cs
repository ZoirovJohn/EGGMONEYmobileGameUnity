using UnityEngine;

public class RotateImageLikeAnim : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float minRotation = -20f;
    [SerializeField] private float maxRotation = 20f;
    [SerializeField] private float duration = 1.5f; // Time for one complete cycle
    
    private float originalZRotation;
    private float timer = 0f;
    
    void Start()
    {
        originalZRotation = transform.localEulerAngles.z;
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Using PingPong to smoothly go back and forth between 0 and 1
        float t = Mathf.PingPong(timer / duration, 1f);
        
        // Interpolate between min and max rotation
        float currentRotation = Mathf.Lerp(minRotation, maxRotation, t);
        
        // Apply the rotation (keeps X and Y rotation unchanged)
        Vector3 currentEuler = transform.localEulerAngles;
        currentEuler.z = originalZRotation + currentRotation;
        transform.localEulerAngles = currentEuler;
    }
}