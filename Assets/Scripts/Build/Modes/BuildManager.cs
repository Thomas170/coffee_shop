using Unity.Netcode;
using UnityEngine;

public class BuildManager : BaseBuildMode
{
    public void ConfirmBuild()
    {
        if (previewManager.preview == null || !previewManager.preview.IsValid) return;

        if (!playerController.playerBuild.IsInMoveMode)
        {
            if (CurrencyManager.Instance.coins < currentBuildable.cost)
            {
                Debug.Log("Pas assez d'argent !");
                return;
            }
            
            CurrencyManager.Instance.RemoveCoins(currentBuildable.cost);
        }

        Vector3 position = previewManager.preview.transform.position;
        Quaternion rotation = previewManager.preview.transform.rotation;

        SpawnBuildableServerRpc(position, rotation);

        BuildSaveData data = new BuildSaveData
        {
            prefabName = currentBuildable.resultPrefab.name,
            position = position,
            rotation = rotation
        };

        SaveData save = SaveManager.Instance.LoadCurrentSlot();
        save.builds.Add(data);
        SaveManager.Instance.SaveData(save);
        
        ExitMode();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildableServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject buildObject = Instantiate(currentBuildable.resultPrefab, position, rotation);
        buildObject.GetComponent<NetworkObject>().Spawn();
        
        ClientSpotManager.Instance.AddSpotsFromBuild(buildObject);
    }
}
