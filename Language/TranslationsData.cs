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
            { "Korean", "상태창" }
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
            { "Korean", "현재 지역 변경" }
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
            { "Korean", "초대 코드는 한 번 입력하면 변경할 수 없습니다. 초대 코드 시스템을 통해 추가 이익을 얻을 수 있습니다. 자세한 내용은 Today's Farm 웹사이트를 방문하세요." }
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

        translations["CheckOurWebsite"] = new()
        {
            { "English", "Check our website" },
            { "Korean",  "웹사이트 확인하기" }
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
            { "Korean", "위의 농장 타일을 클릭하여 아이템을 배치할 수 있어요.\n돼지도 이 정도는 알 수 있어요." }
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

        translations["Store_SelectQuantity"] = new()
        {
            { "English", "Select quantity" },
            { "Korean", "수량을 선택하세요" }
        };

        translations["ItemNotEnough"] = new Dictionary<string, string>
        {
            { "English", "You don't have any {0}.\nPurchase it from the store." },
            { "Korean", "{0}이(가) 없습니다. 상점에서 구매하세요." }
        };

        translations["HowManyToPutCage"] = new Dictionary<string, string>
        {
            { "English", "How many {0} do you want to put to the cage?" },
            { "Korean", "{0}을(를) 몇 개 우리에 넣고 싶나요?" }
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

        translations["BatteryNeedRobot"] = new()
        {
            { "English", "You need a robot first to use batteries!" },
            { "Korean", "배터리를 사용하려면 먼저 로봇이 필요합니다!" }
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

        // 🔹 Item Display Names
        translations["Item_Nest"] = new Dictionary<string, string>
        {
            { "English", "Nest" },
            { "Korean", "둥지" }
        };

        translations["Item_SilverEgg"] = new Dictionary<string, string>
        {
            { "English", "Silver Egg" },
            { "Korean", "실버에그" }
        };

        translations["Item_Food"] = new Dictionary<string, string>
        {
            { "English", "Food" },
            { "Korean", "먹이" }
        };

        translations["Item_GoldEgg"] = new Dictionary<string, string>
        {
            { "English", "Gold Egg" },
            { "Korean", "골드에그" }
        };

        translations["Item_Vitamin"] = new Dictionary<string, string>
        {
            { "English", "Vitamin Booster" },
            { "Korean", "비타민 부스터" }
        };

        translations["Item_Battery"] = new Dictionary<string, string>
        {
            { "English", "Battery" },
            { "Korean", "배터리" }
        };

        translations["Item_Robot"] = new Dictionary<string, string>
        {
            { "English", "Robot" },
            { "Korean", "로봇" }
        };

        translations["Item_SuperBlueEgg"] = new Dictionary<string, string>
        {
            { "English", "Super Blue Egg" },
            { "Korean", "슈퍼 블루 알" }
        };

        translations["Item_SuperRedEgg"] = new Dictionary<string, string>
        {
            { "English", "Super Red Egg" },
            { "Korean", "슈퍼 레드 알" }
        };

        translations["Item_FarmKey"] = new Dictionary<string, string>
        {
            { "English", "Farm Key" },
            { "Korean", "농장 열쇠" }
        };

        translations["Item_SuperBattery"] = new()
        {
            { "English", "Super Battery" },
            { "Korean", "슈퍼 배터리" }
        };

        translations["Item_SuperVitamin"] = new()
        {
            { "English", "Super Vitamin" },
            { "Korean", "슈퍼 비타민" }
        };

        translations["Item_Unknown"] = new()
        {
            { "English", "this item" },
            { "Korean", "이 아이템" }
        };

        translations["RobotTakesBattery"] = new()
        {
            { "English", "Robot takes battery itself, don't worry!" },
            { "Korean", "로봇이 배터리를 자동으로 사용하니 걱정하지 마세요!" }
        };

        // 🔹 Chickens / hens
        translations["Item_WhiteChick"] = new Dictionary<string, string>
        {
            { "English", "White Chick" },
            { "Korean", "흰 병아리" }
        };

        translations["Item_ChampChick"] = new Dictionary<string, string>
        {
            { "English", "Champion Chick" },
            { "Korean", "챔피언 병아리" }
        };

        // open new farm section

        translations["ErrorNoKey"] = new Dictionary<string, string>
        {
            { "English", "You don’t have this key.\nPurchase it from the store." },
            { "Korean", "해당 열쇠가 없습니다. 상점에서 구매하세요." }
        };

        translations["ConfirmOpenNewFarm"] = new Dictionary<string, string>
        {
            { "English", "Would you want to open a new farm?" },
            { "Korean", "새 농장을 여시겠습니까?" }
        };

        translations["Key_Farm"] = new Dictionary<string, string>
        {
            { "English", "Farm Key" },
            { "Korean", "농장 열쇠" }
        };

        translations["Key_PremiumFarm"] = new Dictionary<string, string>
        {
            { "English", "Premium Farm Key" },
            { "Korean", "프리미엄 농장 열쇠" }
        };

        // Farm level keys
        translations["Key_Farm_4"] = new Dictionary<string, string>
        {
            { "English", "Farm 4 Key" },
            { "Korean", "농장 4 열쇠" }
        };

        translations["Key_Farm_5"] = new Dictionary<string, string>
        {
            { "English", "Farm 5 Key" },
            { "Korean", "농장 5 열쇠" }
        };

        translations["Key_Farm_6"] = new Dictionary<string, string>
        {
            { "English", "Farm 6 Key" },
            { "Korean", "농장 6 열쇠" }
        };

        translations["Key_Farm_7"] = new Dictionary<string, string>
        {
            { "English", "Farm 7 Key" },
            { "Korean", "농장 7 열쇠" }
        };

        translations["Key_Farm_8"] = new Dictionary<string, string>
        {
            { "English", "Farm 8 Key" },
            { "Korean", "농장 8 열쇠" }
        };

        // lock slot click info

        translations["SelectFarmKeyFirst"] = new Dictionary<string, string>
        {
            { "English", "Would you want to open a new farm?\nPlease select a farm key first!" },
            { "Korean", "새 농장을 여시겠습니까? 먼저 농장 열쇠를 선택하세요!" }
        };

        // INVENTORY - Info Hatch Panel

        // Hatch Panel Text
        translations["Hatch_YouHave"] = new Dictionary<string, string>
        {
            { "English", "You have: {0} {1}" },
            { "Korean", "보유한 수량: {0} {1}" }
        };

        translations["Hatch_Available"] = new Dictionary<string, string>
        {
            { "English", "Available for hatch" },
            { "Korean", "부화 가능합니다" }
        };

        translations["Hatch_NotEnough"] = new Dictionary<string, string>
        {
            { "English", "Don't have enough eggs" },
            { "Korean", "달걀이 부족합니다" }
        };

        // Egg names
        translations["Egg_Silver"] = new Dictionary<string, string>
        {
            { "English", "Silver Eggs" },
            { "Korean", "실버에그" }
        };

        translations["Egg_Gold"] = new Dictionary<string, string>
        {
            { "English", "Gold Eggs" },
            { "Korean", "골드에그" }
        };

        translations["Egg_Generic"] = new Dictionary<string, string>
        {
            { "English", "eggs" },
            { "Korean", "달걀" }
        };

        translations["HATCH"] = new Dictionary<string, string>
        {
            { "English", "HATCH" },
            { "Korean", "부화" }
        };

        // STORE DB

        translations["Store_nest_Title"] = new Dictionary<string, string> {
            { "English", "NEST" },
            { "Korean", "둥지" }
        };

        translations["Store_nest_Desc"] = new Dictionary<string, string> {
            { "English", "Chickens can’t lay eggs without this." },
            { "Korean", "닭들이 알을 낳으려면 둥지를 먼저 깔아줘야 해요" }
        };

        translations["Store_battery_Title"] = new Dictionary<string, string> {
            { "English", "Battery" },
            { "Korean", "배터리" }
        };

        translations["Store_battery_Desc"] = new Dictionary<string, string> {
            { "English", "Batteries are a food to moving a robot." },
            { "Korean", "로봇을 작동하려면 배터리가 필요해요" }
        };

        translations["Store_vitamin_Title"] = new Dictionary<string, string> {
            { "English", "Vitamin" },
            { "Korean", "비타민" }
        };

        translations["Store_vitamin_Desc"] = new Dictionary<string, string> {
            { "English", "Egg laying speed increases" },
            { "Korean", "닭들에게 주면 알을 낳는 속도가 증가해요." }
        };

        translations["Store_food_Title"] = new Dictionary<string, string> {
            { "English", "Prey" },
            { "Korean", "닭 모이" }
        };

        translations["Store_food_Desc"] = new Dictionary<string, string> {
            { "English", "If you don’t feed a chicken, it will die." },
            { "Korean", "먹이가 떨어지지 않도록 충분히 준비하세요." }
        };

        translations["Store_robot_Title"] = new Dictionary<string, string> {
            { "English", "Robot" },
            { "Korean", "로봇" }
        };

        translations["Store_robot_Desc"] = new Dictionary<string, string> {
            { "English", "The farm’s all–rounder" },
            { "Korean", "청소하기, 먹이주기,  달걀모으기 나에게 맡겨주세요." }
        };

        translations["Store_silver_egg_Title"] = new Dictionary<string, string> {
            { "English", "Sgg" },
            { "Korean", "실버 알" }
        };

        translations["Store_silver_egg_Desc"] = new Dictionary<string, string> {
            { "English", "Growing up from a chick to a gentle hen" },
            { "Korean", "부화시키면 얘쁜 순둥이를 만날 수 있어요." }
        };

        translations["Store_gold_egg_Title"] = new Dictionary<string, string> {
            { "English", "Ggg" },
            { "Korean", "황금 알" }
        };

        translations["Store_gold_egg_Desc"] = new Dictionary<string, string> {
            { "English", "Growing up from a chick to a Champ" },
            { "Korean", "부화시키면 멋진 챔프를 만날 수 있어." }
        };

        translations["Store_super_blue_egg_Title"] = new Dictionary<string, string> {
            { "English", "Blue Egg" },
            { "Korean", "파란 알" }
        };

        translations["Store_super_blue_egg_Desc"] = new Dictionary<string, string> {
            { "English", "You can get premium items." },
            { "Korean", "부화 시키면 여러가지 고급 아이템들을 얻을 수 있어요." }
        };

        translations["Store_super_red_egg_Title"] = new Dictionary<string, string> {
            { "English", "Red Egg" },
            { "Korean", "빨간 알" }
        };

        translations["Store_super_red_egg_Desc"] = new Dictionary<string, string> {
            { "English", "You can get a special chicken." },
            { "Korean", "부화키시면 희귀한 닭을 얻을 수 있답니다." }
        };

        translations["Store_farmKey_Title"] = new Dictionary<string, string> {
            { "English", "Farm Key" },
            { "Korean", "농장 오픈키" }
        };

        translations["Store_farmKey_Desc"] = new Dictionary<string, string> {
            { "English", "Opens a new farm." },
            { "Korean", "잠겨있는 농장을 열수 더 많은 닭을 키울 수 있어요." }
        };

        translations["BUY"] = new Dictionary<string, string> {
            { "English", "BUY" },
            { "Korean", "구매" }
        };

        // STORE - Purchase Popup

        translations["Store_Price"] = new() {
            { "English", "Price : {0} FP" },
            { "Korean", "가격 : {0} FP" }
        };

        translations["Store_Total"] = new() {
            { "English", "Total : {0} FP" },
            { "Korean", "총액 : {0} FP" }
        };

        translations["Store_NotPurchasable"] = new() {
            { "English", "This item cannot be purchased." },
            { "Korean", "이 아이템은 구매할 수 없습니다." }
        };

        translations["Store_NoWallet"] = new() {
            { "English", "Wallet not found." },
            { "Korean", "지갑을 찾을 수 없습니다." }
        };

        translations["Store_ClickToBuy"] = new() {
            { "English", "Available for purchase" },
            { "Korean", "구매 가능" }
        };

        translations["Store_InsufficientFP"] = new() {
            { "English", "Not enough FP balance." },
            { "Korean", "FP 잔액이 부족합니다." }
        };

        translations["Store_Processing"] = new() {
            { "English", "Purchased" },
            { "Korean", "구매 완료" }
        };

        translations["Store_PurchaseFailed"] = new() {
            { "English", "Purchase failed. Please try again." },
            { "Korean", "구매에 실패했습니다. 다시 시도하세요." }
        };

        translations["Store_Purchased"] = new() {
            { "English", "Purchase completed!" },
            { "Korean", "구매 완료!" }
        };

        translations["Store_ItemNotFound"] = new() {
            { "English", "Item not found." },
            { "Korean", "아이템을 찾을 수 없습니다." }
        };

        translations["Status_Robot"] = new() {
            { "English", "Robot: {0}" },
            { "Korean", "로봇: {0}" }
        };

        translations["Status_Battery"] = new() {
            { "English", "Batteries: {0} days" },
            { "Korean", "배터리: {0}일" }
        };

        // SWAP & DELIVER

        translations["Swap"] = new Dictionary<string, string>
        {
            { "English", "Swap" },
            { "Korean", "교환" }
        };

        translations["Delivery"] = new Dictionary<string, string>
        {
            { "English", "Delivery" },
            { "Korean", "배송" }
        };

        translations["SwapText1"] = new Dictionary<string, string>
        {
            { "English", "You can get " },
            { "Korean", "획득 가능: " }
        };

        translations["SwapText2"] = new Dictionary<string, string>
        {
            { "English", "Would you like to exchange the selected quantity of eggs for FP?" },
            { "Korean", "선택한 수량의 알을 FP로 교환하시겠습니까?" }
        };

        translations["DeliveryText1"] = new Dictionary<string, string>
        {
            { "English", "Fill in 30 and order delivery" },
            { "Korean", "30개를 채워서 배송 주문하세요" }
        };

        translations["Confirm"] = new Dictionary<string, string>
        {
            { "English", "Confirm" },
            { "Korean", "확인" }
        };

        translations["Cancel"] = new Dictionary<string, string>
        {
            { "English", "Cancel" },
            { "Korean", "취소" }
        };

        translations["Put"] = new()
        {
            { "English", "Put" },
            { "Korean", "놓기" }
        };

        // InfoFarmItself panel
        translations["Farm_Robot"] = new()
        {
            { "English", "Robot" },
            { "Korean", "로봇" }
        };

        translations["Farm_Hen"] = new()
        {
            { "English", "Hen" },
            { "Korean", "순둥" }
        };

        translations["Farm_Champ"] = new()
        {
            { "English", "Champ" },
            { "Korean", "챔프" }
        };

        translations["Farm_Nest"] = new()
        {
            { "English", "Nest" },
            { "Korean", "둥지" }
        };

        translations["Farm_Information"] = new()
        {
            { "English", "Farm Information" },
            { "Korean", "농장 정보" }
        };

        translations["NotEnoughFreeNest"] = new()
        {
            { "English", "Not enough free nests. Available nests: {0}" },
            { "Korean", "빈 둥지가 부족합니다. 사용 가능한 둥지: {0}" }
        };

        translations["NotEnoughFarmNestSpace_Generic"] = new()
        {
            { "English", "Not enough space in this farm." },
            { "Korean", "농장에 공간이 부족합니다." }
        };



        return translations;
    }
}