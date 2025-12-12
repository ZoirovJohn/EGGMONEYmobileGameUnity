using UnityEngine;

public class BounceAnimation : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] float bounceHeight = 20f; // How high to bounce (in pixels)
    [SerializeField] float bounceSpeed = 2f; // How fast to bounce (higher = faster)
    [SerializeField] AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smooth curve
    
    [Header("Options")]
    [SerializeField] bool bounceOnEnable = true; // Start bouncing when enabled
    [SerializeField] bool continuousBounce = true; // Keep bouncing forever
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private float time = 0f;
    private bool isBouncing = false;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }
    
    void OnEnable()
    {
        if (bounceOnEnable)
        {
            StartBouncing();
        }
    }
    
    void OnDisable()
    {
        StopBouncing();
    }
    
    void Update()
    {
        if (isBouncing && rectTransform != null)
        {
            time += Time.deltaTime * bounceSpeed;
            
            // Calculate bounce using sine wave (0 to PI for upward motion only)
            float normalizedTime = time % (Mathf.PI * 2);
            float bounce = Mathf.Max(0, Mathf.Sin(normalizedTime)) * bounceHeight;
            
            // Apply curve for more natural movement
            float curveValue = bounceCurve.Evaluate(Mathf.Sin(normalizedTime));
            bounce = bounce * curveValue;
            
            // Update position (only upward from original position)
            rectTransform.anchoredPosition = originalPosition + new Vector2(0, bounce);
            
            // If not continuous, stop after one complete bounce cycle
            if (!continuousBounce && time >= Mathf.PI)
            {
                StopBouncing();
            }
        }
    }
    
    public void StartBouncing()
    {
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
            time = 0f;
            isBouncing = true;
        }
    }
    
    public void StopBouncing()
    {
        isBouncing = false;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    public void SetBounceHeight(float height)
    {
        bounceHeight = height;
    }
    
    public void SetBounceSpeed(float speed)
    {
        bounceSpeed = speed;
    }
    
    public void SetContinuousBounce(bool continuous)
    {
        continuousBounce = continuous;
    }
}