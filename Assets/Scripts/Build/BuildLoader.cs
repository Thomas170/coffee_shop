using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BuildLoader : MonoBehaviour
{
    [SerializeField] private List<BuildableDefinition> allBuildables;

    private void Start()
    {
        LoadBuilds();
    }

    public void LoadBuilds()
    {
        SaveData data = SaveManager.Instance.LoadCurrentSlot();
        if (data.builds == null) return;

        foreach (BuildSaveData build in data.builds)
        {
            BuildableDefinition definition = allBuildables.Find(b => b.resultPrefab.name == build.prefabName);
            if (definition == null)
            {
                Debug.LogWarning($"Prefab not found: {build.prefabName}");
                continue;
            }

            GameObject go = Instantiate(definition.resultPrefab, build.position, build.rotation);
            if (go.TryGetComponent(out NetworkObject netObj))
                netObj.Spawn();
        }
    }
}