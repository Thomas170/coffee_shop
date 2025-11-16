using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine.UI;

public class ClientCommands : NetworkBehaviour
{
    [SerializeField] private Transform handPoint;
    [SerializeField] private GameObject orderIcon;
    [SerializeField] private SmoothGaugeUI waitingGauge;
    [SerializeField] private ClientController clientController;
    [SerializeField] private GameObject resultItemPrefab;
    
    public ItemBase currentItem;
    public GameObject commandSpot;
    
    private readonly float _patienceTime = 200f;
    
    public OrderType currentOrder;
    [SerializeField] private OrderList possibleOrders;

    private Coroutine _drinkCoffeeCoroutine;
    private bool _isSodaClient;
    public bool IsSodaClient() => _isSodaClient;

    private void Start()
    {
         orderIcon.SetActive(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void InitCommandSpotServerRpc()
    {
        if (_isSodaClient)
        {
            GameObject targetDispenser = SodaDispenserManager.Instance.GetRandomDispenser();
            if (targetDispenser == null)
            {
                _isSodaClient = false;
            }
            else
            {
                SodaDispenser sodaDispenser = targetDispenser.GetComponent<SodaDispenser>();
                clientController.movement.MoveTo(sodaDispenser.targetPoint.transform);
                return;
            }
        }
        
        commandSpot = ClientSpotManager.Instance.RequestSpot(gameObject);
        if (commandSpot == null)
        {
            clientController.clientSpawner.DespawnClient(gameObject);
            return;
        }
        clientController.movement.MoveTo(commandSpot.transform);
        ClientSpot clientSpot = commandSpot.GetComponent<ClientSpot>();
        SyncCommandSpotClientRpc(clientSpot.buildParent.GetComponent<NetworkObject>(), clientSpot.index);
    }

    [ClientRpc]
    private void SyncCommandSpotClientRpc(NetworkObjectReference buildRef, int childIndex)
    {
        if (buildRef.TryGet(out NetworkObject build))
        {
            ClientSpot[] spots = build.GetComponentsInChildren<ClientSpot>(true);

            foreach (ClientSpot spot in spots)
            {
                if (spot.index == childIndex)
                {
                    commandSpot = spot.gameObject;
                    break;
                }
            }

            if (commandSpot == null) Debug.LogWarning($"ClientSpot with index {childIndex} not found on {build.name}");
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartOrderServerRpc() => StartOrderClientRpc();
    
    [ClientRpc]
    private void StartOrderClientRpc()
    {
        clientController.canInteract = true;
        orderIcon.SetActive(true);
        
        currentOrder = possibleOrders.GetRandomOrder();
        Image orderImage = orderIcon.transform.Find("OrderImage").GetComponent<Image>();
        orderImage.sprite = currentOrder.orderIcon;

        if (!clientController.isTuto)
        {
            waitingGauge.StartGauge(_patienceTime);
            waitingGauge.OnEmpty = LeaveCoffeeShop;
        }
    }

    public void GiveItem(ItemBase itemToUse)
    {
        if (IsValidItemToUse(itemToUse))
        {
            RequestGiveItemServerRpc(itemToUse.NetworkObject, NetworkManager.Singleton.LocalClientId);
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
        
        waitingGauge.StopGauge();
        orderIcon.SetActive(false);
        
        currentItem = itemBase;
        currentItem.CurrentHolderClientId = null;
        currentItem.AttachTo(ClientSpotManager.Instance.GetItemSpotLocation(commandSpot), false);
        
        _drinkCoffeeCoroutine = StartCoroutine(DrinkCoffee());
        CurrencyManager.Instance.AddCoins(currentOrder.price);
        LevelManager.Instance.GainExperience(currentOrder.experience);
        clientController.canInteract = false;

        if (clientController.isTuto) TutorialManager.Instance.ValidStep(TutorialStep.GiveCupClient);
    }

    private IEnumerator DrinkCoffee()
    {
        yield return new WaitForSeconds(10f);
        LeaveCoffeeShop();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SpawnResultItemServerRpc()
    {
        currentItem.NetworkObject.Despawn();
        Destroy(currentItem.gameObject);
        
        GameObject resultItem = Instantiate(resultItemPrefab, ClientSpotManager.Instance.GetItemSpotLocation(commandSpot).position, Quaternion.identity);
        NetworkObject networkObject = resultItem.GetComponent<NetworkObject>();
        networkObject.Spawn();

        SpawnResultItemClientRpc(networkObject);
    }

    [ClientRpc]
    private void SpawnResultItemClientRpc(NetworkObjectReference itemRef)
    {
        if (!itemRef.TryGet(out var itemNetworkObject)) return;
        currentItem = itemNetworkObject.GetComponent<ItemBase>();
        currentItem.AttachTo(ClientSpotManager.Instance.GetItemSpotLocation(commandSpot));
    }

    public void LeaveCoffeeShop()
    {
        if (currentItem)
        {
            SpawnResultItemServerRpc();
        }
        
        waitingGauge.StopGauge();
        orderIcon.SetActive(false);
        clientController.canInteract = false;
        
        if (_drinkCoffeeCoroutine != null)
        {
            StopCoroutine(_drinkCoffeeCoroutine);
            _drinkCoffeeCoroutine = null;
        }
        
        if (IsServer)
        {
            ClientSpotManager.Instance.ReleaseSpot(commandSpot);
        }
        
        GetComponent<NavMeshAgent>().enabled = true;
        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }
    
    private bool IsValidItemToUse(ItemBase itemToUse)
    {
        if (itemToUse == null || currentOrder == null) return false;
        return itemToUse.itemType == currentOrder.requiredItemType;
    }
    
    public void SetSodaClient()
    {
        _isSodaClient = true;
    }
    
    public void PurchaseSodaAndLeave()
    {
        CurrencyManager.Instance.AddCoins(10);
        LevelManager.Instance.GainExperience(10);

        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }
}
