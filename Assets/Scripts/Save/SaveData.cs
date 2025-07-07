using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int level;
    public int experience;
    public int coins;
    public List<BuildSaveData> builds = new();

    public SaveData(int level = 1, int experience = 0, int coins = 0)
    {
        this.level = level;
        this.experience = experience;
        this.coins = coins;
    }
}