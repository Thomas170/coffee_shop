using Unity.Netcode;
using UnityEngine;

public class BuildManager : BaseBuildMode
{
    public void ConfirmBuild()
    {
        if (previewManager.preview == null || !previewManager.preview.IsValid) return;

        if (!playerController.playerBuild.IsInMoveMode && CurrencyManager.Instance.coins < currentBuildable.cost)
        {
            Debug.Log("Pas assez d'argent !");
            return;
        }

        if (!playerController.playerBuild.IsInMoveMode)
        {
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
        CurrencyManager.Instance.RemoveCoins(currentBuildable.cost);

        save.builds.Add(data);
        SaveManager.Instance.SaveData(save);

        ClientSpotManager.Instance.RefreshSpotsFromScene();
        ExitMode();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBuildableServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject buildObject = Instantiate(currentBuildable.resultPrefab, position, rotation);
        buildObject.GetComponent<NetworkObject>().Spawn();
    }
}
