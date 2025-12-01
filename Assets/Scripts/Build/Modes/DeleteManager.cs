using System;
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
        ulong networkId = editManager.targetedBuild.GetComponent<NetworkObject>().NetworkObjectId;

        // Demander au serveur de supprimer ET de gérer l'argent
        BuildableDefinition definition = editManager.targetedBuild.GetComponent<BuildableReference>().definition;
        bool isMoving = editManager.playerController.playerBuild.IsInMoveMode;
        
        DeleteBuildServerRpc(
            networkId, 
            prefabName, 
            position, 
            rotation, 
            definition.cost,
            isMoving,
            NetworkManager.Singleton.LocalClientId
        );
        
        editManager.ClearPreviousHighlight();
        editManager.ExitMode();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeleteBuildServerRpc(
        ulong networkId, 
        string prefabName, 
        Vector3 position, 
        Quaternion rotation,
        int originalCost,
        bool isMoving,
        ulong clientId)
    {
        // Supprimer l'objet
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var netObj))
        {
            ClientSpotManager.Instance.RemoveSpotsFromBuild(netObj.gameObject);
            netObj.Despawn();
            Destroy(netObj.gameObject);
        }

        // Rembourser (côté serveur uniquement)
        if (!isMoving)
        {
            int returnMoney = (int)Math.Floor(originalCost * 0.75f);
            // Utilise directement la NetworkVariable côté serveur
            if (CurrencyManager.Instance.IsServer)
            {
                CurrencyManager.Instance.AddCoins(returnMoney);
            }
        }

        // Sauvegarder (côté serveur uniquement)
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