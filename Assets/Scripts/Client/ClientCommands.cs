using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class ClientCommands : NetworkBehaviour
{
    [SerializeField] private Transform handPoint;
    [SerializeField] private GameObject orderIcon;
    [SerializeField] private SmoothGaugeUI waitingGauge;
    [SerializeField] private ClientController clientController;
    [SerializeField] private GameObject resultItemPrefab;
    
    public ItemBase currentItem;
    public Transform commandSpot;
    
    private readonly float _patienceTime = 40f;

    private void Start()
    {
         orderIcon.SetActive(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void InitCommandSpotServerRpc()
    {
        commandSpot = ClientBarSpotManager.Instance.RequestSpot();
        if (commandSpot == null)
        {
            clientController.clientSpawner.DespawnClient(gameObject);
            return;
        }
        clientController.movement.MoveTo(commandSpot);
        //SyncSpot
        SyncCommandSpotClientRpc();
    }

    [ClientRpc]
    public void SyncCommandSpotClientRpc()
    {
        //ClientBarSpotManager.Instance.SyncSpot();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartOrderServerRpc() => StartOrderClientRpc();
    
    [ClientRpc]
    public void StartOrderClientRpc()
    {
        clientController.canInteract = true;
        orderIcon.SetActive(true);

        waitingGauge.StartGauge(_patienceTime);
        waitingGauge.OnEmpty = LeaveCoffeeShop;
    }

    public void GiveItem(ItemBase itemToUse)
    {
        if (IsValidItemToUse(itemToUse))
        {
            RequestGiveItemServerRpc(itemToUse.NetworkObject, NetworkManager.LocalClientId);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestGiveItemServerRpc(NetworkObjectReference itemRef, ulong playerId) => RequestGiveItemClientRpc(itemRef, playerId);
    
    [ClientRpc]
    private void RequestGiveItemClientRpc(NetworkObjectReference itemRef, ulong playerId)
    {
        if (currentItem || !itemRef.TryGet(out var itemNetworkObject)) return;

        ItemBase itemBase = itemNetworkObject.GetComponent<ItemBase>();
        
        PlayerController player = PlayerListManager.Instance.GetPlayer(playerId);
        PlayerCarry playerCarry = player.GetComponent<PlayerCarry>();
        playerCarry.carriedItem = null;
        
        orderIcon?.SetActive(false);
        currentItem = itemBase;
        currentItem.CurrentHolderClientId = null;
        currentItem.AttachTo(commandSpot.Find("CupSpot").transform, false);
        
        StartCoroutine(DrinkCoffee());
        CurrencyManager.Instance.AddCoins(10);
    }

    private IEnumerator DrinkCoffee()
    {
        yield return new WaitForSeconds(5f);

        SpawnResultItemServerRpc();
        LeaveCoffeeShop();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SpawnResultItemServerRpc()
    {
        currentItem.NetworkObject.Despawn();
        Destroy(currentItem.gameObject);
        
        GameObject resultItem = Instantiate(resultItemPrefab, commandSpot.Find("CupSpot").position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentItem = itemNetworkObject.GetComponent<ItemBase>();
        currentItem.AttachTo(commandSpot.Find("CupSpot"));
    }

    private void LeaveCoffeeShop()
    {
        ClientBarSpotManager.Instance.ReleaseSpot(commandSpot);
        orderIcon.SetActive(false);
        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }
    
    private bool IsValidItemToUse(ItemBase itemToUse)
    {
        return itemToUse != null && itemToUse.itemType == ItemType.CupFull;
    }
}
