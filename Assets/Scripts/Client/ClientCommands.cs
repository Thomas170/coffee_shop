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
    public int commandSpotIndex;
    
    private readonly float _patienceTime = 40f;

    private void Start()
    {
         orderIcon.SetActive(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void InitCommandSpotServerRpc()
    {
        commandSpotIndex = ClientBarSpotManager.Instance.RequestSpot();
        if (commandSpotIndex == -1)
        {
            clientController.clientSpawner.DespawnClient(gameObject);
            return;
        }
        clientController.movement.MoveTo(ClientBarSpotManager.Instance.GetClientSpotLocation(commandSpotIndex));
        SyncCommandSpotClientRpc(commandSpotIndex);
    }

    [ClientRpc]
    public void SyncCommandSpotClientRpc(int index)
    {
        commandSpotIndex = index;
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
        playerCarry.TryDrop();
        playerCarry.carriedItem = null;
        
        orderIcon.SetActive(false);
        currentItem = itemBase;
        currentItem.CurrentHolderClientId = null;
        currentItem.AttachTo(ClientBarSpotManager.Instance.GetItemSpotLocation(commandSpotIndex), false);
        
        StartCoroutine(DrinkCoffee());
        CurrencyManager.Instance.AddCoins(10);
    }

    private IEnumerator DrinkCoffee()
    {
        yield return new WaitForSeconds(20f);

        SpawnResultItemServerRpc();
        yield return new WaitForSeconds(1f);
        LeaveCoffeeShop();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SpawnResultItemServerRpc()
    {
        currentItem.NetworkObject.Despawn();
        Destroy(currentItem.gameObject);
        
        GameObject resultItem = Instantiate(resultItemPrefab, ClientBarSpotManager.Instance.GetItemSpotLocation(commandSpotIndex).position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentItem = itemNetworkObject.GetComponent<ItemBase>();
        currentItem.AttachTo(ClientBarSpotManager.Instance.GetItemSpotLocation(commandSpotIndex));
    }

    private void LeaveCoffeeShop()
    {
        if (IsServer)
        {
            ClientBarSpotManager.Instance.ReleaseSpot(commandSpotIndex);
        }
        
        orderIcon.SetActive(false);
        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }
    
    private bool IsValidItemToUse(ItemBase itemToUse)
    {
        return itemToUse != null && itemToUse.itemType == ItemType.CupFull;
    }
}
