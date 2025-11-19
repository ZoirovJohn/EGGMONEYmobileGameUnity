using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class PlayerDataLoader : MonoBehaviour
{
    [Header("References")]
    public AuthManager authManager;
    public BasketManager basketManager;
    public PlayerWallet playerWallet;
    
    [Header("Farm Data Loading")]
    public FarmAPIManager farmAPIManager;
    public FarmDatabase farmDatabase;
    public FarmHeaderManager farmHeaderManager;

    private void Start()
    {
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        string accessToken = AuthStorage.GetAccessToken();

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogWarning("⚠️ No access token found. User not authenticated.");
            SceneManager.LoadScene("Bootstrap");
            return;
        }

        authManager.GetMe(
            onSuccess: (response) =>
            {
                Debug.Log("✅ Player data loaded: " + response);

                MeResponse userData = JsonUtility.FromJson<MeResponse>(response);

                if (playerWallet != null)
                {
                    playerWallet.SetName(userData.nickName);
                    playerWallet.SetLocation(userData.nation);
                    playerWallet.SetVideo(userData.video);
                    playerWallet.SetUserFarms(userData.userFarms);
                    playerWallet.SetReferralCode(userData.referralCode);
                    
                    if (int.TryParse(userData.userFP, out int fp))
                        playerWallet.SetFP(fp);
                    else
                        playerWallet.SetFP(0);

                    Debug.Log($"💰 PlayerWallet updated: {userData.nickName}, {userData.nation}, {userData.userFP} FP, {userData.video} videos, {userData.userFarms} farms, Referral: {userData.referralCode}");
                }
                else
                {
                    Debug.LogError("❌ PlayerWallet reference is missing!");
                }

                PlayerPrefs.SetString("userId", userData.id);
                PlayerPrefs.SetString("email", userData.email);
                PlayerPrefs.SetString("nickname", userData.nickName);
                PlayerPrefs.SetInt("userFarms", userData.userFarms);
                PlayerPrefs.SetString("referralCode", userData.referralCode);
                PlayerPrefs.Save();

                // ✅ Load basket data
                LoadBasketData();
                
                // ✅ Load farm data from backend
                LoadFarmData(userData.userFarms);
            },
            onError: (err) =>
            {
                Debug.LogError("❌ Failed to load player data: " + err);
                SceneManager.LoadScene("Bootstrap");
            }
        );
    }

    private void LoadBasketData()
    {
        if (basketManager == null)
        {
            Debug.LogWarning("⚠️ BasketManager not assigned, skipping basket load");
            return;
        }

        basketManager.GetBasket(
            onSuccess: (response) =>
            {
                Debug.Log("🧺 Basket data loaded: " + response);

                BasketResponse basketData = JsonUtility.FromJson<BasketResponse>(response);

                if (playerWallet != null)
                {
                    playerWallet.SetEggs(basketData.eggCount);
                    Debug.Log($"🥚 Eggs in basket: {basketData.eggCount}");
                }
            },
            onError: (err) =>
            {
                Debug.LogError("❌ Failed to load basket data: " + err);
            }
        );
    }

    /// <summary>
    /// ✅ Load all farm data from backend
    /// </summary>
    private void LoadFarmData(int farmCount)
    {
        if (farmCount <= 0)
        {
            Debug.LogWarning("⚠️ User has 0 farms, skipping farm data load");
            return;
        }

        if (farmAPIManager == null)
        {
            Debug.LogWarning("⚠️ FarmAPIManager not assigned, skipping farm data load");
            return;
        }

        if (farmDatabase == null)
        {
            Debug.LogError("❌ FarmDatabase not assigned!");
            return;
        }

        Debug.Log($"📡 Loading {farmCount} farms from backend...");

        farmAPIManager.LoadAllFarmSummaries(
            farmCount,
            onAllLoaded: (summaries) =>
            {
                Debug.Log($"✅ All {farmCount} farms loaded successfully!");
                ConvertAndStoreFarmData(summaries);
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to load farm data: {error}");
                // Fallback to JSON data if available
                if (farmDatabase.farmDataJSON != null)
                {
                    Debug.Log("📂 Falling back to local JSON data");
                    farmDatabase.LoadFromJSON();
                }
            }
        );
    }

    /// <summary>
    /// Convert backend farm summaries to FarmData and store in database
    /// </summary>
    private void ConvertAndStoreFarmData(FarmSummary[] summaries)
    {
        if (summaries == null || summaries.Length == 0)
        {
            Debug.LogError("❌ No farm summaries to convert!");
            return;
        }

        farmDatabase.farms.Clear();

        for (int i = 0; i < summaries.Length; i++)
        {
            FarmSummary summary = summaries[i];
            FarmData farmData = ConvertSummaryToFarmData(summary, i);
            farmDatabase.farms.Add(farmData);
        }

        // Generate cages from farm data
        farmDatabase.GenerateCagesFromFarmData();

        Debug.Log($"✅ Loaded {farmDatabase.farms.Count} farms into FarmDatabase");

        // Refresh UI
        if (farmHeaderManager != null)
        {
            farmHeaderManager.Refresh();
            Debug.Log("🔄 FarmHeaderManager refreshed");
        }
        else
        {
            Debug.LogWarning("⚠️ FarmHeaderManager not assigned, UI not refreshed");
        }
    }

    /// <summary>
    /// ✅ FIXED: Convert a single FarmSummary to FarmData with correct nest count
    /// </summary>
    private FarmData ConvertSummaryToFarmData(FarmSummary summary, int index)
    {
        FarmData farm = new FarmData
        {
            farmIndex = index,
            farmId = $"farm_{(index + 1):D3}", // farm_001, farm_002, etc.
            farmName = $"Farm {summary.farm.farmNumber}",
            
            // ✅ Backend farm data
            isPremium = summary.farm.isPremium,
            maxCapacity = summary.farm.maxCapacity,
            
            // ✅✅ FIXED: Use summary.nests.total instead of occupied
            // This gives the TOTAL number of nests (10), not just occupied ones (2)
            nestsOccupied = summary.nests.total,  // ✅ CORRECT!
            
            cages = new System.Collections.Generic.List<CageData>()
        };

        // ✅ Count chicks by kind from backend hen stats
        farm.normalChicks = summary.henStats.byKind.Normal;
        farm.champChicks = summary.henStats.byKind.Champ;
        farm.legendChicks = summary.henStats.byKind.Legend;
        farm.superLegendChicks = summary.henStats.byKind.SuperLegend;
        
        // ✅ Count premium and normal nests
        int premiumNests = 0;
        int normalNests = 0;
        
        if (summary.nests.details != null)
        {
            foreach (var nest in summary.nests.details)
            {
                if (nest.isPremium)
                    premiumNests++;
                else
                    normalNests++;
            }
        }
        
        farm.premiumNests = premiumNests;
        farm.normalNests = normalNests;
        
        Debug.Log($"🪺 Nest breakdown: {premiumNests} premium, {normalNests} normal (Total: {summary.nests.total})");
        
        // ✅ Robot data from backend
        if (summary.robot != null)
        {
            farm.hasRobot = !string.IsNullOrEmpty(summary.robot.id);
            farm.robotActive = summary.robot.isActive;
            farm.robotPoweredUntil = summary.robot.poweredUntil ?? "";
            
            // Update robotType for compatibility
            if (farm.hasRobot)
            {
                farm.robotType = "robot";
            }
        }
        else
        {
            farm.hasRobot = false;
            farm.robotActive = false;
            farm.robotPoweredUntil = "";
            farm.robotType = "none";
        }

        // ✅ Set farmKeyType based on isPremium
        if (farm.isPremium)
        {
            farm.farmKeyType = "premiumfarmkey";
        }
        else
        {
            farm.farmKeyType = "normal";
        }

        Debug.Log($"🐔 Farm {summary.farm.farmNumber}: " +
                  $"Nests: {farm.nestsOccupied}/{farm.maxCapacity} ({premiumNests} premium, {normalNests} normal), " +
                  $"Normal: {farm.normalChicks}, " +
                  $"Champ: {farm.champChicks}, " +
                  $"Legend: {farm.legendChicks}, " +
                  $"SuperLegend: {farm.superLegendChicks}, " +
                  $"Premium: {farm.isPremium}, " +
                  $"HasRobot: {farm.hasRobot}, " +
                  $"RobotActive: {farm.robotActive}");

        return farm;
    }

    [Serializable]
    private class MeResponse
    {
        public string id;
        public string nickName;
        public string email;
        public string phoneNumber;
        public string firebaseUid;
        public string nation;
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

    [Serializable]
    private class BasketResponse
    {
        public int eggCount;
    }
}