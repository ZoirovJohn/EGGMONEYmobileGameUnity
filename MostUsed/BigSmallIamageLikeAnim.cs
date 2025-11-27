using UnityEngine;

public class BigSmallImageLikeAnim : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.0f;
    [SerializeField] private float duration = 1.5f; // Time for one complete cycle (big->small->big)
    
    private Vector3 originalScale;
    private float timer = 0f;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Using PingPong to smoothly go back and forth between 0 and 1
        float t = Mathf.PingPong(timer / duration, 1f);
        
        // Interpolate between min and max scale
        float currentScale = Mathf.Lerp(minScale, maxScale, t);
        
        // Apply the scale (keeps the original scale proportions)
        transform.localScale = originalScale * currentScale;
    }
}