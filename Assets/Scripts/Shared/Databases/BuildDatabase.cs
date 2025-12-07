using System.Collections.Generic;
using UnityEngine;

public class BuildDatabase : MonoBehaviour
{
    public static BuildDatabase Instance { get; private set; }
    public List<BuildableDefinition> Builds { get; private set; } = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllBuilds();
    }

    private void LoadAllBuilds()
    {
        BuildableDefinition[] builds = Resources.LoadAll<BuildableDefinition>("Builds");
        Builds = new List<BuildableDefinition>(builds);

        Debug.Log($"[BuildDatabase] {Builds.Count} build(s) chargés.");
    }

    public BuildableDefinition GetBuildByName(string buildName)
    {
        return Builds.Find(b => b.resultPrefab.name == buildName);
    }
}