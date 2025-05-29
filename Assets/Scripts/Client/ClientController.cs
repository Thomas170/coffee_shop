using UnityEngine;

public class ClientController : MonoBehaviour
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

        commands.InitCommandSpotServerRpc();
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
        if (reachedTarget == commands.commandSpot.transform)
        {
            commands.StartOrderServerRpc();
        }
        else if (clientSpawner.IsExitPoint(reachedTarget))
        {
            clientSpawner.DespawnClient(gameObject);
        }
    }
}
