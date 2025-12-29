using UnityEngine;
using UnityEngine.UI;

public class CenterAreaFlow : MonoBehaviour
{
    [Header("Refs")]
    public GameObject instructionsPanel;   
    public GameObject farmPanel;           
    public Toggle dontShowAgainToggle; 

    [Header("Managers")]
    public InventoryManager inventoryManager;
    public PlayerWallet playerWallet;

    private const string PREFS_KEY = "HideInstructions";

    [Header("Dev (Editor Only)")]
    [SerializeField] private bool resetSkipOnPlay = false; 

    private void Awake()
    {
    #if UNITY_EDITOR
        if (resetSkipOnPlay)
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            PlayerPrefs.Save();
        }
    #endif
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>(FindObjectsInactive.Include);
        
        if (playerWallet == null)
            playerWallet = FindAnyObjectByType<PlayerWallet>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        if (instructionsPanel) instructionsPanel.SetActive(false);
        if (farmPanel)         farmPanel.SetActive(false);

        if (GameAudioManager.Instance != null)
        {
            Debug.Log("[CenterAreaFlow] ✅ BGM will auto-start from GameAudioManager");
        }
        else
        {
            Debug.LogWarning("[CenterAreaFlow] ⚠️ GameAudioManager not found! Add AudioManager to scene.");
        }

        StartCoroutine(LoadInventoryDataDelayed());
        ShowInstructionsIfNeeded();

        if (dontShowAgainToggle)
        {
            dontShowAgainToggle.isOn = false;
            dontShowAgainToggle.onValueChanged.AddListener(OnSkipToggleChanged);
        }
    }

    private System.Collections.IEnumerator LoadInventoryDataDelayed()
    {
        yield return null;
        LoadInventoryData();
    }

    private void LoadInventoryData()
    {
        if (inventoryManager == null)
        {
            return;
        }

        if (!inventoryManager.gameObject.activeInHierarchy)
        {
            return;
        }

        inventoryManager.GetInventory(
            onSuccess: (response) =>
            {
                Debug.Log("[CenterAreaFlow] ✅ Inventory data loaded successfully!");
            },
            onError: (err) =>
            {
                Debug.LogError($"[CenterAreaFlow] ❌ Failed to load inventory: {err}");
            }
        );
    }

    private void ShowInstructionsIfNeeded()
    {
        bool skip = PlayerPrefs.GetInt(PREFS_KEY, 0) == 1;
        if (skip)
        {
            OpenFarm(); 
        }
        else
        {
            if (farmPanel) farmPanel.SetActive(false);
            if (instructionsPanel) instructionsPanel.SetActive(true);
        }
    }

    private void OnSkipToggleChanged(bool isOn)
    {
        if (!isOn || instructionsPanel == null || !instructionsPanel.activeSelf) return;

        PlayerPrefs.SetInt(PREFS_KEY, 1);
        PlayerPrefs.Save();

        instructionsPanel.SetActive(false);
        OpenFarm();
    }

    private void OpenFarm()
    {
        if (farmPanel) farmPanel.SetActive(true);
    }
}