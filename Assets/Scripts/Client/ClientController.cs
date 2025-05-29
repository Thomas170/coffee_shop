using UnityEngine;

public class ClientController : MonoBehaviour
{
    public ClientMovement movement;
    public ClientCommands commands;
    public ClientSpawner clientSpawner;

    private void Start()
    {
        movement = GetComponent<ClientMovement>();
        commands = GetComponent<ClientCommands>();
        clientSpawner = GameObject.FindWithTag("GameManager").GetComponent<ClientSpawner>();

        commands.InitCommandSpot();
    }

    public void OnDestinationReached(Transform reachedTarget)
    {
        if (reachedTarget == commands.commandSpot.transform)
        {
            commands.StartOrder();
        }
        else if (clientSpawner.IsExitPoint(reachedTarget))
        {
            clientSpawner.DespawnClient(gameObject);
        }
    }
}
