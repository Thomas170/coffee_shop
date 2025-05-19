public struct SaveSlotData
{
    public bool HasData;
    public int  Level;

    public SaveSlotData(bool hasData, int level = 0)
    {
        HasData = hasData;
        Level   = level;
    }
}