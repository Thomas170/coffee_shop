using UnityEngine;
using System.Collections.Generic;

public enum OptionType
{
    Slider,
    ButtonGroup
}

[System.Serializable]
public class SettingsOption
{
    public OptionType type;
    public List<GameObject> elements;
    public GameObject background;
}