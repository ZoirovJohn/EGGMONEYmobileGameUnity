using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CageData
{
    public int id;
    public int farmIndex;
    public int nestCapacity;
    public int nestsOccupied;
    public int normalChicks;
    public int champChicks;
    public bool hasEgg;           // Randomly assigned by system
    public float remainingTime;   // Randomly assigned by system
    public int upgradeLevel;
}

[System.Serializable]
public class FarmData
{
    public string farmId;        // Add this line
    public string farmName;
    public int farmIndex;
    public int nestsOccupied;
    public int normalChicks;
    public int champChicks;
    public List<CageData> cages = new List<CageData>();
}