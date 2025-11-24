using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CharacterVideoSelector : MonoBehaviour
{
    [Header("References")]
    public CentreAreaPager pager;       // Your existing pager component
    public Button nextButton;           // The "Next" button
    public AuthManager authManager;     // Your auth manager
    
    private int selectedVideoIndex = 1; // Default to first video (1-indexed for backend)

    void Start()
    {
        // Setup next button
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonPressed);
        }
    }

    void Update()
    {
        // Get current page from pager (0-indexed)
        // Convert to 1-indexed for backend (1, 2, 3, 4)
        if (pager != null)
        {
            selectedVideoIndex = GetCurrentPageIndex() + 1;
        }
    }

    // Helper method to get current page from pager
    private int GetCurrentPageIndex()
    {
        if (pager == null || pager.scrollRect == null) return 0;
        
        float pos = pager.scrollRect.horizontalNormalizedPosition;
        int pageCount = pager.pageCount;
        
        // Calculate which page we're closest to
        int nearestPage = 0;
        float minDist = float.MaxValue;
        
        for (int i = 0; i < pageCount; i++)
        {
            float pagePos = (pageCount == 1) ? 0f : (float)i / (pageCount - 1);
            float dist = Mathf.Abs(pos - pagePos);
            
            if (dist < minDist)
            {
                minDist = dist;
                nearestPage = i;
            }
        }
        
        return nearestPage;
    }

    public void OnNextButtonPressed()
    {
        // Update video property via API
        UpdateUserVideo(selectedVideoIndex);
    }

    void UpdateUserVideo(int videoIndex)
    {
        // Create update data - only video field
        UserUpdateData updateData = new UserUpdateData { video = videoIndex };
        string jsonData = JsonUtility.ToJson(updateData);

        // Call /auth/update endpoint
        authManager.UpdateUser(
            jsonData,
            onSuccess: (response) =>
            {
                // Parse response to get updated user data
                UpdateResponse userData = JsonUtility.FromJson<UpdateResponse>(response);
                
                // Update PlayerPrefs
                PlayerPrefs.SetInt("video", userData.video);
                PlayerPrefs.Save();
                
                // Update PlayerWallet
                PlayerWallet wallet = FindFirstObjectByType<PlayerWallet>();
                if (wallet != null)
                {
                    wallet.SetVideo(userData.video);
                }
                
                // Load Farm scene
                SceneManager.LoadScene("Farm");
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to update user: {error}");
                
                // Still proceed to Farm scene even if update fails
                SceneManager.LoadScene("Farm");
            }
        );
    }

    [Serializable]
    private class UserUpdateData
    {
        public int video;
        // Can also include: nickName, phoneNumber, nation if needed
    }

    [Serializable]
    private class UpdateResponse
    {
        public string id;
        public string email;
        public string firebaseUid;
        public int video;
        public bool emailVerified;
        public string totpSecret;
        public bool is2FAEnabled;
        public string createdAt;
        public string updatedAt;
        public string userFP;
        public string referralCode;
        public string referredByCode;
        public int userFarms;
    }
}