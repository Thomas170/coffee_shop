using Unity.Netcode;
using UnityEngine;

public class DeleteManager : MonoBehaviour
{
    public EditManager editManager;
    public void TryDelete()
    {
        if (editManager.targetedBuild == null) return;

        string prefabName = editManager.targetedBuild.name.Replace("(Clone)", "").Trim();
        Vector3 position = editManager.targetedBuild.transform.position;
        Quaternion rotation = editManager.targetedBuild.transform.rotation;

        RemoveBuild(prefabName, position, rotation);
    }

    private void RemoveBuild(string buildName, Vector3 position, Quaternion rotation)
    {
        DeleteBuildServerRpc(editManager.targetedBuild.GetComponent<NetworkObject>().NetworkObjectId);
        editManager.ClearPreviousHighlight();

        BuildSaveData data = new BuildSaveData
        {
            prefabName = buildName,
            position = position,
            rotation = rotation
        };

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Remove(data);
        SaveManager.Instance.SaveData(save);
        
        editManager.ExitMode();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeleteBuildServerRpc(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
        {
            netObj.Despawn();
            Destroy(netObj.gameObject);
        }
    }
}
