using UnityEngine;
using System;

public static class AuthStorage
{
    private const string AccessTokenKey = "access_token";
    private const string ExpiryKey = "access_token_expiry";

    private static readonly TimeSpan tokenLifetime = TimeSpan.FromMinutes(30);

    public static void SaveAccessToken(string token)
    {
        PlayerPrefs.SetString(AccessTokenKey, token);
        DateTime expiry = DateTime.UtcNow.Add(tokenLifetime);
        PlayerPrefs.SetString(ExpiryKey, expiry.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public static string GetAccessToken()
    {
        if (!PlayerPrefs.HasKey(AccessTokenKey) || !PlayerPrefs.HasKey(ExpiryKey))
            return null;

        string token = PlayerPrefs.GetString(AccessTokenKey);
        long binary;
        if (!long.TryParse(PlayerPrefs.GetString(ExpiryKey), out binary))
            return null;

        DateTime expiry = DateTime.FromBinary(binary);
        if (DateTime.UtcNow > expiry)
        {
            DeleteAccessToken();
            return null;
        }

        return token;
    }

    public static void DeleteAccessToken()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(ExpiryKey);
        PlayerPrefs.Save();
    }

    public static bool IsTokenValid()
    {
        return !string.IsNullOrEmpty(GetAccessToken());
    }
}
