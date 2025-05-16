using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MenuEntry
{
    public Button button;
    [HideInInspector] public Image backgroundImage;
}

public enum HighlightMode
{
    SelectionOnly,
    AlwaysVisible
}