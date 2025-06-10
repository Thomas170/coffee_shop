using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int level;
    public int coins;
    public List<BuildSaveData> builds = new();

    public SaveData(int level = 1, int coins = 0)
    {
        this.level = level;
        this.coins = coins;
    }
}