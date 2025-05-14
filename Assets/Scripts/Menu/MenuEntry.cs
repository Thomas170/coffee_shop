using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MenuEntry
{
    public Button button;
    public bool isClickable = true;
    [HideInInspector] public Image backgroundImage;
}