using Unity.Netcode;
using UnityEngine;

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
        if (reachedTarget == ClientBarSpotManager.Instance.GetClientSpotLocation(commands.commandSpotIndex))
        {
            commands.StartOrderServerRpc();
        }
        else if (clientSpawner.IsExitPoint(reachedTarget))
        {
            clientSpawner.DespawnClient(gameObject);
        }
    }
}
