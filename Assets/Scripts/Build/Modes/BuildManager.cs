using Unity.Netcode;
using UnityEngine;

public class BuildManager : BaseBuildMode
{
    private bool _isWaitingForPurchase;

    public void ConfirmBuild()
    {
        if (previewManager.preview == null || !previewManager.preview.IsValid) return;
        if (_isWaitingForPurchase) return;

        Vector3 position = previewManager.preview.transform.position;
        Quaternion rotation = previewManager.preview.transform.rotation;

        if (!playerController.playerBuild.IsInMoveMode)
        {
            _isWaitingForPurchase = true;
            
            CurrencyManager.Instance.TryPurchase(
                currentBuildable.cost,
                onSuccess: () => OnPurchaseSuccess(position, rotation),
                onFailure: OnPurchaseFailure
            );
        }
        else
        {
            // En mode déplacement, pas d'achat nécessaire
            FinalizeBuild(position, rotation);
        }
    }

    private void OnPurchaseSuccess(Vector3 position, Quaternion rotation)
    {
        _isWaitingForPurchase = false;
        FinalizeBuild(position, rotation);
    }

    private void OnPurchaseFailure()
    {
        _isWaitingForPurchase = false;
        Debug.Log("Fonds insuffisants !");
    }

    private void FinalizeBuild(Vector3 position, Quaternion rotation)
    {
        SpawnBuildableServerRpc(
            currentBuildable.resultPrefab.name,
            position,
            rotation,
            NetworkManager.Singleton.LocalClientId
        );
        
        ExitMode();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBuildableServerRpc(string prefabName, Vector3 position, Quaternion rotation, ulong clientId)
    {
        BuildableDefinition definition = BuildDatabase.Instance.Builds.Find(b => b.resultPrefab.name == prefabName);
        if (definition == null)
        {
            Debug.LogError($"Prefab not found: {prefabName}");
            return;
        }

        GameObject buildObject = Instantiate(definition.resultPrefab, position, rotation);
        buildObject.GetComponent<NetworkObject>().Spawn();
        
        ClientSpotManager.Instance.AddSpotsFromBuild(buildObject);

        BuildSaveData data = new BuildSaveData
        {
            prefabName = prefabName,
            position = position,
            rotation = rotation
        };

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Add(data);
        SaveManager.Instance.SaveData(save);
    }
}