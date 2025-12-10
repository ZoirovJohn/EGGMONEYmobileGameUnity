using System.Collections.Generic;

public static class TranslationsData
{
    public static Dictionary<string, Dictionary<string, string>> GetTranslations()
    {
        var translations = new Dictionary<string, Dictionary<string, string>>();

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

        // Add more translations here as needed

        return translations;
    }
}