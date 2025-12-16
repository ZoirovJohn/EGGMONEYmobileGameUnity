using System;
using System.Collections.Generic;

[Serializable]
public class FarmSummary
{
    public FarmInfo farm;
    public RobotInfo robot;
    public NestsInfo nests;
    public HenStats henStats;
}

[Serializable]
public class FarmInfo
{
    public int farmNumber; 
    public bool isPremium;
    public int maxCapacity;
}

    [Serializable]
    public class RobotInfo
    {
        public string id;
        public bool isActive;
        public string poweredUntil;
        public bool needsCharge;
        public string batteryType; // ✅ NEW: "normal" or "premium"
        public int daysLeftToNextCharge; // ✅ NEW: Direct days count from backend
        public string lastUsed;
    }

[Serializable]
public class NestsInfo
{
    public int total;
    public int occupied;
    public int empty;
    public int availableSlots;
    public NestDetail[] details;
}

[Serializable]
public class NestDetail
{
    public string nestId;
    public bool isPremium;
    public bool isEmpty;
    public bool needsCleaning;
    public int readyEggs;
    public HenInfo hen;
}

[Serializable]
public class HenInfo
{
    public string id;
    public string kind; // Normal, Champ, Legend, SuperLegend
    public string stage; // chick, hen, retired
    public bool alive;
    public int lifetimeDaysRemaining;
    public float baseSpeed;
    public float eggProgress;
    public int pendingEggs;
    public bool hasEggReady;
    public string eggReadyAt;
    public bool foodGiven;
    public bool cleaned;
    public string vitaminType; // normal, premium
    public string vitaminExpiresAt;
    public string layingStartedAt;
}

[Serializable]
public class HenStats
{
    public int total;
    public HenByKind byKind;
    public HenByStage byStage;
    public int withEggsReady;
    public int needsFood;
    public int needsCleaning;
}

[Serializable]
public class HenByKind
{
    public int Normal;
    public int Champ;
    public int Legend;
    public int SuperLegend;
}

[Serializable]
public class HenByStage
{
    public int chick;
    public int hen;
    public int retired;
}