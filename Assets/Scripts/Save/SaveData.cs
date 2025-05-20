using System;

[Serializable]
public class SaveData
{
    public int level;
    public int coins;

    public SaveData(int level = 1, int coins = 0)
    {
        this.level = level;
        this.coins = coins;
    }
}