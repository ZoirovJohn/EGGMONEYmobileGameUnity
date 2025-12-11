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
                }

                PlayerPrefs.SetString("userId", userData.id);
                PlayerPrefs.SetString("email", userData.email);
                PlayerPrefs.SetString("nickname", userData.nickName);
                PlayerPrefs.SetInt("userFarms", userData.userFarms);
                PlayerPrefs.SetString("referralCode", userData.referralCode);
                PlayerPrefs.Save();

                LoadBasketData();
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
                BasketResponse basketData = JsonUtility.FromJson<BasketResponse>(response);

                if (playerWallet != null)
                {
                    playerWallet.SetEggs(basketData.eggCount);
                }
            },
            onError: (err) =>
            {
                Debug.LogError("❌ Failed to load basket data: " + err);
            }
        );
    }

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

        farmAPIManager.LoadAllFarmSummaries(
            farmCount,
            onAllLoaded: (summaries) =>
            {
                ConvertAndStoreFarmData(summaries);
            },
            onError: (error) =>
            {
                Debug.LogError($"❌ Failed to load farm data: {error}");
                if (farmDatabase.farmDataJSON != null)
                {
                    farmDatabase.LoadFromJSON();
                }
            }
        );
    }

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

            // store nest details for cage generation
            if (summary.nests != null && summary.nests.details != null)
            {
                farmDatabase.SetFarmNestDetails(i, summary.nests.details);
            }
        }

        farmDatabase.GenerateCagesFromFarmData();

        if (farmHeaderManager != null)
            farmHeaderManager.Refresh();
    }

    private FarmData ConvertSummaryToFarmData(FarmSummary summary, int index)
    {
        FarmData farm = new FarmData
        {
            farmIndex = index,
            farmId = $"farm_{(index + 1):D3}",
            farmName = $"Farm {summary.farm.farmNumber}",
            isPremium = summary.farm.isPremium,
            maxCapacity = summary.farm.maxCapacity,
            nestsOccupied = summary.nests.total,
            cages = new System.Collections.Generic.List<CageData>()
        };

        // Chick counts
        farm.normalChicks = summary.henStats.byKind.Normal;
        farm.champChicks = summary.henStats.byKind.Champ;
        farm.legendChicks = summary.henStats.byKind.Legend;
        farm.superLegendChicks = summary.henStats.byKind.SuperLegend;

        // PREMIUM / NORMAL NEST COUNTS (THE FIX YOU NEEDED)
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

        farm.premiumNests = premiumNests;   // 🌟 STORED CORRECTLY HERE
        farm.normalNests = normalNests;

        // Robot
        if (summary.robot != null)
        {
            farm.hasRobot = !string.IsNullOrEmpty(summary.robot.id);
            farm.robotActive = summary.robot.isActive;
            farm.robotPoweredUntil = summary.robot.poweredUntil ?? "";

            if (farm.hasRobot)
                farm.robotType = "robot";
        }
        else
        {
            farm.hasRobot = false;
            farm.robotActive = false;
            farm.robotPoweredUntil = "";
            farm.robotType = "none";
        }

        farm.farmKeyType = farm.isPremium ? "premiumfarmkey" : "normal";

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
    public class BasketResponse
    {
        public int eggCount;
    }
}
