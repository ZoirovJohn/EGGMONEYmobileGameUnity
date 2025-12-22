using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class EggExchangeManager : MonoBehaviour
{
    [Header("Config")]
    public APIConfig config;

    [Header("References")]
    [SerializeField] private PlayerWallet wallet;
    [SerializeField] private InfoErrorChanger infoErrorChanger;
    [SerializeField] private BasketManager basketManager;
    [SerializeField] private Button topButton;
    [SerializeField] private Button topButton2;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField eggInputField;

    [Header("Add Buttons")]
    [SerializeField] private Button btnAdd1;
    [SerializeField] private Button btnAdd5;
    [SerializeField] private Button btnAdd10;
    [SerializeField] private Button btnReset; // acts like minus → reset to 0

    [SerializeField] private TMP_Text txtFP;
    [SerializeField] private Button btnYes;

    [Header("Localized Text")]
    [SerializeField] private TMP_Text swapText1; // "You can get {FP} FP"

    [Header("Exchange Settings")]
    [SerializeField] private int fpPerEgg = 400;

    private int availableEggs = 0;
    private int currentInputAmount = 0;
    private bool isPanelOpen = false;

    private void Start()
    {
        btnAdd1?.onClick.AddListener(() => AddEggs(1));
        btnAdd5?.onClick.AddListener(() => AddEggs(5));
        btnAdd10?.onClick.AddListener(() => AddEggs(10));
        btnReset?.onClick.AddListener(ResetInput);

        btnYes?.onClick.AddListener(OnYesButtonClicked);

        eggInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        eggInputField.onValueChanged.AddListener(OnInputValueChanged);

        if (topButton != null && topButton2 != null)
        {
            topButton.onClick.AddListener(OnTopButtonClicked);
            topButton2.onClick.AddListener(OnTopButtonClicked);
        }

        UpdateSwapText();
    }

    private void OnEnable()
    {
        UpdateSwapText();
    }

    private void OnTopButtonClicked()
    {
        if (isPanelOpen)
            CloseExchangePanel();
        else
            OpenExchangePanel();
    }

    private void OpenExchangePanel()
    {
        isPanelOpen = true;

        basketManager.GetBasket(
            onSuccess: OnBasketFetchSuccess,
            onError: OnBasketFetchError
        );
    }

    private void CloseExchangePanel()
    {
        isPanelOpen = false;
        infoErrorChanger?.CloseAllInfoErrorMethod();
        ResetInput();
    }

    private void OnBasketFetchSuccess(string json)
    {
        BasketResponse response = JsonUtility.FromJson<BasketResponse>(json);
        availableEggs = response.eggCount;

        infoErrorChanger?.OpenInfoExchangeFPCoin();
        ResetInput();
    }

    private void OnBasketFetchError(string error)
    {
        infoErrorChanger?.OpenErrorDefault("Failed to load egg data.");
        isPanelOpen = false;
    }

    // =====================
    // Egg Logic
    // =====================

    private void AddEggs(int amount)
    {
        int newValue = currentInputAmount + amount;

        if (newValue > availableEggs)
        {
            infoErrorChanger?.OpenErrorDefault($"You only have {availableEggs} eggs");
            newValue = availableEggs;
        }

        currentInputAmount = newValue;
        UpdateInputDisplay();
    }

    private void OnInputValueChanged(string value)
    {
        if (!int.TryParse(value, out int parsed))
            parsed = 0;

        currentInputAmount = Mathf.Clamp(parsed, 0, availableEggs);
        UpdateFPDisplay();
    }

    private void ResetInput()
    {
        currentInputAmount = 0;
        UpdateInputDisplay();
    }

    private void UpdateInputDisplay()
    {
        eggInputField.text = currentInputAmount.ToString();
        UpdateFPDisplay();
    }

    private void UpdateFPDisplay()
    {
        long totalFP = (long)currentInputAmount * fpPerEgg;
        txtFP.text = totalFP.ToString("N0");
        UpdateSwapText();
    }

    private void UpdateSwapText()
    {
        UpdateLocalizedTexts();
    }

    public void UpdateLocalizedTexts()
    {
        if (LanguageManager.Instance == null) return;

        // Base localized text like "You can get {0} FP"
        string baseText = LanguageManager.Instance.GetTranslation("SwapText1");

        long totalFP = (long)currentInputAmount * fpPerEgg;

        // Supports {0} formatting if you want later
        swapText1.text = string.Format(baseText, totalFP.ToString("N0"));
    }

    // =====================
    // Confirm / Cancel
    // =====================

    private void OnYesButtonClicked()
    {
        if (currentInputAmount <= 0)
        {
            infoErrorChanger?.OpenErrorDefault("Please select eggs to exchange");
            return;
        }

        int eggs = currentInputAmount;
        int fp = eggs * fpPerEgg;

        wallet.TrySpendEggs(eggs);
        wallet.Add(fp);

        availableEggs -= eggs;

        infoErrorChanger?.OpenErrorDefault(
            $"Success! Exchanged {eggs} eggs for {fp:N0} FP"
        );

        ResetInput();
        StartCoroutine(ClosePanelAfterDelay(2f));
        StartCoroutine(SyncExchangeWithBackend(eggs));
    }

    private IEnumerator ClosePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseExchangePanel();
    }

    // =====================
    // Backend Sync
    // =====================

    private IEnumerator SyncExchangeWithBackend(int eggAmount)
    {
        string token = AuthStorage.GetAccessToken();
        if (string.IsNullOrEmpty(token)) yield break;

        string url = config.baseUrl + "/economy/basket-to-fp";

        var body = JsonUtility.ToJson(new BasketToFPRequest { eggs = eggAmount });

        using var req = new UnityEngine.Networking.UnityWebRequest(url, "POST");
        req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(
            System.Text.Encoding.UTF8.GetBytes(body)
        );
        req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + token);

        yield return req.SendWebRequest();
    }

    [Serializable]
    public class BasketResponse
    {
        public int eggCount;
    }
}
