using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Loads the correct character video to ALL Video Players based on PlayerWallet.Video property (1-4)
/// Handles multiple Video Players showing the same video
/// </summary>
public class CharacterVideoLoader : MonoBehaviour
{
    [Header("All Video Players (drag all of them here)")]
    [Tooltip("Add all 4-5 Video Player components that should show the same video")]
    [SerializeField] private VideoPlayer[] videoPlayers;
    
    [Header("Character Video Clips (1-4)")]
    [Tooltip("Assign video for character 1")]
    [SerializeField] private VideoClip video1;
    
    [Tooltip("Assign video for character 2")]
    [SerializeField] private VideoClip video2;
    
    [Tooltip("Assign video for character 3")]
    [SerializeField] private VideoClip video3;
    
    [Tooltip("Assign video for character 4")]
    [SerializeField] private VideoClip video4;
    
    [Header("PlayerWallet Reference")]
    [SerializeField] private PlayerWallet playerWallet;
    
    [Header("Options")]
    [SerializeField] private bool autoFindVideoPlayers = true;
    [SerializeField] private bool syncPlayback = true;
    
    void Start()
    {
        // Auto-find all Video Players in scene if enabled
        if (autoFindVideoPlayers && (videoPlayers == null || videoPlayers.Length == 0))
        {
            videoPlayers = FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
        }
        
        // Find PlayerWallet if not assigned
        if (playerWallet == null)
        {
            playerWallet = FindFirstObjectByType<PlayerWallet>();
        }
        
        // Subscribe to PlayerWallet changes
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged += OnPlayerProfileChanged;
        }
        
        // Try to load video immediately (in case data is already loaded)
        LoadCharacterVideo();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerWallet != null)
        {
            playerWallet.OnProfileChanged -= OnPlayerProfileChanged;
        }
    }
    
    /// <summary>
    /// Called when PlayerWallet profile changes (including after backend load)
    /// </summary>
    private void OnPlayerProfileChanged()
    {
        LoadCharacterVideo();
    }
    
    /// <summary>
    /// Loads the video to ALL Video Players based on PlayerWallet.Video property
    /// </summary>
    public void LoadCharacterVideo()
    {
        // Find PlayerWallet if not assigned
        if (playerWallet == null)
        {
            playerWallet = FindFirstObjectByType<PlayerWallet>();
        }
        
        if (playerWallet == null)
        {
            return;
        }
        
        if (videoPlayers == null || videoPlayers.Length == 0)
        {
            return;
        }
        
        // Get the user's video property (1, 2, 3, or 4)
        int userVideo = playerWallet.Video;
        
        // Select the correct video clip
        VideoClip selectedClip = userVideo switch
        {
            1 => video1,
            2 => video2,
            3 => video3,
            4 => video4,
            _ => video1 // Default to video 1 if invalid
        };
        
        if (selectedClip == null)
        {
            return;
        }
        
        // Assign the same video clip to ALL Video Players
        int successCount = 0;
        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp != null)
            {
                vp.clip = selectedClip;
                
                // Optional: Sync playback settings
                if (syncPlayback)
                {
                    vp.isLooping = true;
                    vp.playOnAwake = false;
                }
                
                successCount++;
            }
        }
        
        // Start playing all videos
        if (syncPlayback)
        {
            PlayAllVideos();
        }
    }
    
    /// <summary>
    /// Plays all Video Players (useful for synchronized playback)
    /// </summary>
    public void PlayAllVideos()
    {
        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp != null && !vp.isPlaying)
            {
                vp.Play();
            }
        }
    }
    
    /// <summary>
    /// Pauses all Video Players
    /// </summary>
    public void PauseAllVideos()
    {
        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp != null && vp.isPlaying)
            {
                vp.Pause();
            }
        }
    }
    
    /// <summary>
    /// Stops all Video Players
    /// </summary>
    public void StopAllVideos()
    {
        foreach (VideoPlayer vp in videoPlayers)
        {
            if (vp != null)
            {
                vp.Stop();
            }
        }
    }
    
    /// <summary>
    /// Changes video at runtime (useful for testing or character switching)
    /// </summary>
    public void ChangeVideo(int videoNumber)
    {
        if (playerWallet != null)
        {
            playerWallet.SetVideo(videoNumber);
            LoadCharacterVideo();
        }
    }
}