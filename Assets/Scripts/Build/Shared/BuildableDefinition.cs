using UnityEngine;

[CreateAssetMenu(menuName = "Build/Buildable Definition")]
public class BuildableDefinition : ScriptableObject
{
    public GameObject previewPrefab;
    public GameObject resultPrefab;
    public int cost;
    public Sprite icon;
    public BuildType type;
    public int level;
}