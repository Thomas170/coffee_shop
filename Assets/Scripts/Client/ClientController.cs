using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class ClientController : NetworkBehaviour
{
    public ClientMovement movement;
    public ClientCommands commands;
    public ClientSpawner clientSpawner;
    public bool canInteract;

    private void Start()
    {
        movement = GetComponent<ClientMovement>();
        commands = GetComponent<ClientCommands>();
        clientSpawner = GameObject.FindWithTag("GameManager").GetComponent<ClientSpawner>();

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
        if (reachedTarget == ClientSpotManager.Instance.GetClientSpotLocation(commands.commandSpotIndex))
        {
            GetComponent<NavMeshAgent>().enabled = false;
            
            transform.position = new Vector3(transform.position.x, 10f, transform.position.z);
            GameObject spot = ClientSpotManager.Instance.GetSpot(commands.commandSpotIndex);
            transform.rotation = spot.transform.rotation;
            
            commands.StartOrderServerRpc();
        }
        else if (clientSpawner.IsExitPoint(reachedTarget))
        {
            clientSpawner.DespawnClient(gameObject);
        }
    }
}
