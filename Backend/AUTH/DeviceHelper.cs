using UnityEngine;
using System;

public static class DeviceHelper
{
    private const string DeviceIdKey = "DeviceID";

    public static string GetDeviceId()
    {
        if (PlayerPrefs.HasKey(DeviceIdKey))
            return PlayerPrefs.GetString(DeviceIdKey);

        string newId = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(DeviceIdKey, newId);
        PlayerPrefs.Save();
        return newId;
    }
}
