using System;
using Unity.Netcode;
using UnityEngine;

public class DeleteManager : NetworkBehaviour
{
    public EditManager editManager;
    
    public void TryDelete()
    {
        if (editManager.targetedBuild == null) return;

        string prefabName = editManager.targetedBuild.name.Replace("(Clone)", "").Trim();
        Vector3 position = editManager.targetedBuild.transform.position;
        Quaternion rotation = editManager.targetedBuild.transform.rotation;
        ulong networkId = editManager.targetedBuild.GetComponent<NetworkObject>().NetworkObjectId;

        BuildableDefinition definition = editManager.targetedBuild.GetComponent<BuildableReference>().definition;
        
        bool isMoving = editManager.playerController.playerBuild.IsInMoveMode;
        
        DeleteBuildServerRpc(
            networkId, 
            prefabName, 
            position, 
            rotation, 
            definition.cost,
            isMoving
        );
        
        editManager.ClearPreviousHighlight();
        editManager.ExitMode();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void DeleteBuildServerRpc(
        ulong networkId, 
        string prefabName, 
        Vector3 position, 
        Quaternion rotation,
        int originalCost,
        bool isMoving)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
        {
            ClientSpotManager.Instance.RemoveSpotsFromBuild(netObj.gameObject);
            netObj.Despawn();
            Destroy(netObj.gameObject);
        }

        if (!isMoving)
        {
            int returnMoney = (int)Math.Floor(originalCost * 0.75f);
            CurrencyManager.Instance.AddCoinsServerRpc(returnMoney);
        }

        BuildSaveData data = new BuildSaveData
        {
            prefabName = prefabName,
            position = position,
            rotation = rotation
        };

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Remove(data);
        SaveManager.Instance.SaveData(save);
    }
}