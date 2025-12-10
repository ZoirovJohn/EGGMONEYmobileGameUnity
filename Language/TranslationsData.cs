using System.Collections.Generic;

public static class TranslationsData
{
    public static Dictionary<string, Dictionary<string, string>> GetTranslations()
    {
        var translations = new Dictionary<string, Dictionary<string, string>>();

        // Header Section

        translations["NextGames"] = new Dictionary<string, string>
        {
            { "English", "Next Games" },
            { "Korean", "다음 게임" }
        };

        translations["MiniGames"] = new Dictionary<string, string>
        {
            { "English", "Mini Games" },
            { "Korean", "미니 게임" }
        };

        translations["ComingSoon"] = new Dictionary<string, string>
        {
            { "English", "Coming Soon" },
            { "Korean", "출시 예정" }
        };

        translations["Profile"] = new Dictionary<string, string>
        {
            { "English", "Profile" },
            { "Korean", "프로필" }
        };

        translations["Status"] = new Dictionary<string, string>
        {
            { "English", "Status" },
            { "Korean", "상태" }
        };

        translations["NEXT"] = new Dictionary<string, string>
        {
            { "English", "NEXT" },
            { "Korean", "다음" }
        };

        translations["MINI"] = new Dictionary<string, string>
        {
            { "English", "MINI" },
            { "Korean", "미니" }
        };

        translations["Egg"] = new Dictionary<string, string>
        {
            { "English", "Egg" },
            { "Korean", "알" }
        };

        translations["WithFriends"] = new Dictionary<string, string>
        {
            { "English", "With Friends" },
            { "Korean", "친구와 함께" }
        };

        // Manager Section

        translations["Management"] = new Dictionary<string, string>
        {
            { "English", "Management" },
            { "Korean", "관리" }
        };

        translations["SoundEffects"] = new Dictionary<string, string>
        {
            { "English", "Sound Effects" },
            { "Korean", "효과음" }
        };

        translations["BackgroundMusic"] = new Dictionary<string, string>
        {
            { "English", "Background Music" },
            { "Korean", "배경음악" }
        };

        translations["ChangeYourLocation"] = new Dictionary<string, string>
        {
            { "English", "Change your Location" },
            { "Korean", "위치 변경" }
        };

        translations["SelectCountry"] = new Dictionary<string, string>
        {
            { "English", "Select Country" },
            { "Korean", "국가 선택" }
        };

        translations["Change"] = new Dictionary<string, string>
        {
            { "English", "Change" },
            { "Korean", "변경" }
        };

        translations["LogOut"] = new Dictionary<string, string>
        {
            { "English", "Log Out" },
            { "Korean", "로그아웃" }
        };

        translations["DeleteAccountWarning"] = new Dictionary<string, string>
        {
            { "English", "If you delete your account, you will not be able to create another account for three months. Deleting your account also means deleting your wallet. Before deleting your account, please transfer your wallet assets." },
            { "Korean", "계정을 삭제하면 3개월 동안 다른 계정을 만들 수 없습니다. 계정 삭제는 지갑 삭제를 의미합니다. 계정을 삭제하기 전에 지갑 자산을 이전하세요." }
        };

        translations["MyReferralCode"] = new Dictionary<string, string>
        {
            { "English", "My Referral Code" },
            { "Korean", "내 추천 코드" }
        };

        translations["RegisterReferralCode"] = new Dictionary<string, string>
        {
            { "English", "Register Referral Code" },
            { "Korean", "추천 코드 등록" }
        };

        translations["Copy"] = new Dictionary<string, string>
        {
            { "English", "Copy" },
            { "Korean", "복사" }
        };

        translations["Register"] = new Dictionary<string, string>
        {
            { "English", "Register" },
            { "Korean", "등록" }
        };

        translations["ReferralCodeInfo"] = new Dictionary<string, string>
        {
            { "English", "Once an invite code has been entered, it cannot be changed. You can earn additional eggs through the invite code system. For more information, please visit the Today's Farm website." },
            { "Korean", "초대 코드는 한 번 입력하면 변경할 수 없습니다. 초대 코드 시스템을 통해 추가 알을 얻을 수 있습니다. 자세한 내용은 Today's Farm 웹사이트를 방문하세요." }
        };

        translations["Accept"] = new Dictionary<string, string>
        {
            { "English", "Accept" },
            { "Korean", "수락" }
        };

        translations["Reject"] = new Dictionary<string, string>
        {
            { "English", "Reject" },
            { "Korean", "거절" }
        };

        translations["AccessWallet"] = new Dictionary<string, string>
        {
            { "English", "Access Wallet" },
            { "Korean", "지갑 접속" }
        };

        translations["WalletAccessBlocked"] = new Dictionary<string, string>
        {
            { "English", "Access wallet is not allowed. Change your location" },
            { "Korean", "지갑 접속이 허용되지 않습니다. 위치를 변경하세요" }
        };

        translations["General"] = new Dictionary<string, string>
        {
            { "English", "General" },
            { "Korean", "일반" }
        };

        translations["Referral"] = new Dictionary<string, string>
        {
            { "English", "Referral" },
            { "Korean", "추천" }
        };

        translations["Friends"] = new Dictionary<string, string>
        {
            { "English", "Friends" },
            { "Korean", "친구" }
        };

        translations["Top"] = new Dictionary<string, string>
        {
            { "English", "Top" },
            { "Korean", "상위" }
        };

        translations["Gate"] = new Dictionary<string, string>
        {
            { "English", "Gate" },
            { "Korean", "게이트" }
        };

        translations["JoinUsOnFarm"] = new Dictionary<string, string>
        {
            { "English", "<u>Let's Join Us On Today's Farm</u>" },
            { "Korean", "<u>오늘의 농장에 함께해요</u>" }
        };

        // Farm Section Infos

        translations["FarmTileInstructions"] = new Dictionary<string, string>
        {
            { "English", "You can set up items by clicking on the farm tiles above.\nEven I am a pig, I can figure it out." },
            { "Korean", "위의 농장 타일을 클릭하여 아이템을 배치할 수 있어요.\n저도 돼지지만 이 정도는 알 수 있어요." }
        };

        translations["NotEnoughItem"] = new Dictionary<string, string>
        {
            { "English", "You don't have enough of the item. Purchase it from the store." },
            { "Korean", "아이템이 부족합니다. 상점에서 구매하세요." }
        };

        translations["GoToStore"] = new Dictionary<string, string>
        {
            { "English", "Go To Store" },
            { "Korean", "상점으로 이동" }
        };

        translations["HowManyToPut"] = new Dictionary<string, string>
        {
            { "English", "How many do you want to put?" },
            { "Korean", "몇 개를 놓고 싶나요?" }
        };

        translations["ConfirmPlaceItem"] = new Dictionary<string, string>
        {
            { "English", "Would you like to set the selected item on the farm?" },
            { "Korean", "선택한 아이템을 농장에 배치하시겠습니까?" }
        };

        translations["YES"] = new Dictionary<string, string>
        {
            { "English", "YES" },
            { "Korean", "예" }
        };

        translations["NO"] = new Dictionary<string, string>
        {
            { "English", "NO" },
            { "Korean", "아니요" }
        };

        translations["PutAllVitamins"] = new Dictionary<string, string>
        {
            { "English", "Press the button if you want to put all of your vitamins to hens!" },
            { "Korean", "모든 비타민을 닭들에게 주고 싶다면 버튼을 누르세요!" }
        };

        translations["PUT"] = new Dictionary<string, string>
        {
            { "English", "PUT" },
            { "Korean", "놓기" }
        };

        translations["ItemAlreadyApplied"] = new Dictionary<string, string>
        {
            { "English", "The selected item is already applied to the tile!" },
            { "Korean", "선택한 아이템이 이미 해당 타일에 적용되었습니다!" }
        };

        translations["OpenNewFarm"] = new Dictionary<string, string>
        {
            { "English", "Would you like to open a new farm?" },
            { "Korean", "새 농장을 열고 싶으신가요?" }
        };

        return translations;
    }
}