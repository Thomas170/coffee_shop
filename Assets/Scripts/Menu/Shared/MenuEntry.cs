using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MenuEntry
{
    public Button button;
    [HideInInspector] public Image backgroundImage;
    [HideInInspector] public Image defaultImage;
    [HideInInspector] public Image selectedImage;
}

public enum HighlightMode
{
    SelectionOnly,
    AlwaysVisible
}