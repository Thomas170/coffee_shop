using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ClientController : NetworkBehaviour
{
    public ClientMovement movement;
    public ClientCommands commands;
    public ClientSpawner clientSpawner;
    public bool canInteract;
    public GameObject hightlightRender;

    private void Start()
    {
        movement = GetComponent<ClientMovement>();
        commands = GetComponent<ClientCommands>();
        clientSpawner = GameObject.FindWithTag("GameManager").GetComponent<ClientSpawner>();
        SetHightlight(false);

        if (IsServer)
        {
            commands.InitCommandSpotServerRpc();
        }
    }
    
    public void Interact(ItemBase itemToUse)
    {
        if (!canInteract)
        {
            Debug.LogWarning("Impossible d'intéragir avec cette personne.");
            return;
        }

        commands.GiveItem(itemToUse);
    }

    public void OnDestinationReached(Transform reachedTarget)
    {
        if (commands.IsSodaClient())
        {
            commands.PurchaseSodaAndLeave();
            return;
        }
        
        if (reachedTarget == commands.commandSpot.transform)
        {
            GetComponent<NavMeshAgent>().enabled = false;
            
            Transform spot = commands.commandSpot.transform;
            transform.position = new(spot.position.x, spot.position.y + 4f, spot.position.z);
            transform.rotation = spot.parent.rotation;
            
            commands.StartOrderServerRpc();
        }
        else if (clientSpawner.IsExitPoint(reachedTarget))
        {
            clientSpawner.DespawnClient(gameObject);
        }
    }
    
    public void SetHightlight(bool value)
    {
        if (hightlightRender)
        {
            hightlightRender.SetActive(value);
        }
    }
}
