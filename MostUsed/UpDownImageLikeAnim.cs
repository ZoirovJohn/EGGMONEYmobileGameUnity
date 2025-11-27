using UnityEngine;

public class PulsePosition : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float minOffset = -20f;
    [SerializeField] private float maxOffset = 20f;
    [SerializeField] private float duration = 1.5f; // Time for one complete cycle
    
    private Vector3 originalPosition;
    private float timer = 0f;
    
    void Start()
    {
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Using PingPong to smoothly go back and forth between 0 and 1
        float t = Mathf.PingPong(timer / duration, 1f);
        
        // Interpolate between min and max offset
        float currentOffset = Mathf.Lerp(minOffset, maxOffset, t);
        
        // Apply the position offset on Y axis
        Vector3 newPosition = originalPosition;
        newPosition.y = originalPosition.y + currentOffset;
        transform.localPosition = newPosition;
    }
}