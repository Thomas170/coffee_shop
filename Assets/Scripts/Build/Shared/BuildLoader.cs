using Unity.Netcode;
using UnityEngine;

public class BuildLoader : MonoBehaviour
{
    private void Start()
    {
        LoadBuilds();
        ClientSpotManager.Instance.SetSpotsFromScene();
    }

    public void LoadBuilds()
    {
        SaveData data = SaveManager.Instance.LoadCurrentSlot();
        if (data.builds == null) return;

        foreach (BuildSaveData build in data.builds)
        {
            BuildableDefinition definition = BuildDatabase.Instance.Builds.Find(b => b.resultPrefab.name == build.prefabName);
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