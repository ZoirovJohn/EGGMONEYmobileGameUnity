using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
  [Header("Panels")]
  public GameObject panelIntroVideo;   // 8-second video panel
  public GameObject panelLogin;
  public GameObject panelSignUp;
  public GameObject panelTerms;
  public GameObject panelCharacter;

  [Header("Backgrounds")]
  public GameObject bgSignupLogin;    // used for login, signup, terms (same as bgLogin from UIBootstrap)
  public GameObject bgCharacterHouse;  // used for character select

  [Header("Video Settings")]
  public VideoPlayer introVideoPlayer; // Reference to the Video Player component
  public Button introVideoButton;      // Reference to the Button component on panelIntroVideo

  private void Awake()
  {
    // UIBootstrap logic: Close all panels and backgrounds FIRST
    if (panelIntroVideo != null) panelIntroVideo.SetActive(false);
    if (panelLogin != null) panelLogin.SetActive(false);
    if (panelSignUp != null) panelSignUp.SetActive(false);
    if (panelTerms != null) panelTerms.SetActive(false);
    if (panelCharacter != null) panelCharacter.SetActive(false);

    if (bgSignupLogin != null) bgSignupLogin.SetActive(false);
    if (bgCharacterHouse != null) bgCharacterHouse.SetActive(false);
  }

  private void Start()
  {
    // Show intro video first
    ShowIntroVideo();
    
    // Subscribe to video end event
    if (introVideoPlayer != null)
    {
      introVideoPlayer.loopPointReached += OnVideoFinished;
    }

    // Subscribe to button click for skip functionality
    if (introVideoButton != null)
    {
      introVideoButton.onClick.AddListener(OnIntroVideoClicked);
    }
  }

  private void OnVideoFinished(VideoPlayer vp)
  {
    // When video finishes, show login page
    ShowLogin();
  }

  private void OnIntroVideoClicked()
  {
    // When user clicks during video, skip to login
    ShowLogin();
  }

  public void ShowIntroVideo()
  {
    // Hide ALL panels and backgrounds first
    HideAll();
    
    // ONLY show the intro video panel (no backgrounds)
    if (panelIntroVideo != null) 
    {
      panelIntroVideo.SetActive(true);
    }
    
    // Start playing the video
    if (introVideoPlayer != null)
    {
      introVideoPlayer.Play();
    }
  }

  public void ShowLogin()
  {
    HideAll();
    if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
    if (panelLogin != null) panelLogin.SetActive(true);
  }

  public void ShowSignUp()
  {
    HideAll();
    if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
    if (panelSignUp != null) panelSignUp.SetActive(true);
  }

  public void ShowTerms()
  {
    HideAll();
    if (bgSignupLogin != null) bgSignupLogin.SetActive(true);
    if (panelTerms != null) panelTerms.SetActive(true);
  }

  public void ShowCharacter()
  {
    HideAll();
    if (bgCharacterHouse != null) bgCharacterHouse.SetActive(true);
    if (panelCharacter != null) panelCharacter.SetActive(true);
  }

  private void HideAll()
  {
    // Hide intro video panel
    if (panelIntroVideo != null) panelIntroVideo.SetActive(false);
    
    // Hide all other panels
    if (panelLogin != null) panelLogin.SetActive(false);
    if (panelSignUp != null) panelSignUp.SetActive(false);
    if (panelTerms != null) panelTerms.SetActive(false);
    if (panelCharacter != null) panelCharacter.SetActive(false);

    // Hide all backgrounds
    if (bgSignupLogin != null) bgSignupLogin.SetActive(false);
    if (bgCharacterHouse != null) bgCharacterHouse.SetActive(false);
  }

  private void OnDestroy()
  {
    // Unsubscribe from events to prevent memory leaks
    if (introVideoPlayer != null)
    {
      introVideoPlayer.loopPointReached -= OnVideoFinished;
    }

    if (introVideoButton != null)
    {
      introVideoButton.onClick.RemoveListener(OnIntroVideoClicked);
    }
  }
}