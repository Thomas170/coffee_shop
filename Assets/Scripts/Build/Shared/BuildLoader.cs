using Unity.Netcode;
using UnityEngine;

public class BuildLoader : NetworkBehaviour
{
    private void Start()
    {
        if (IsServer)
        {
            LoadBuilds();
        }
        
        ClientSpotManager.Instance.SetSpotsFromScene();
    }

    private void LoadBuilds()
    {
        SaveData data = SaveManager.Instance.LoadCurrentSlot();
        if (data.builds == null)
        {
            Debug.Log("[BuildLoader] No builds to load.");
            return;
        }

        Debug.Log($"[BuildLoader] Loading {data.builds.Count} builds...");

        foreach (BuildSaveData build in data.builds)
        {
            BuildableDefinition definition = BuildDatabase.Instance.Builds.Find(b => b.resultPrefab.name == build.prefabName);
            if (definition == null)
            {
                Debug.LogWarning($"[BuildLoader] Prefab not found: {build.prefabName}");
                continue;
            }

            GameObject go = Instantiate(definition.resultPrefab, build.position, build.rotation);
            
            if (go.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
                Debug.Log($"[BuildLoader] Spawned build: {build.prefabName}");
            }
        }
    }
}