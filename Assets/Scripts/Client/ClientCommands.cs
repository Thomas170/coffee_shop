using UnityEngine;
using System.Collections;

public class ClientCommands : MonoBehaviour, IInteractable
{
    public ClientController clientController;
    [SerializeField] private Transform handPoint;
    public float patienceTime = 300f;
    public GameObject commandSpot;
    
    private GameObject _heldCup;
    private bool _canInteract;
    private Coroutine _patienceCoroutine;
    
    public void Interact()
    {
        if (!_canInteract)
        {
            Debug.LogWarning("Impossible d'intéragir avec cette personne.");
            return;
        }

        PlayerCarry carry = FindObjectOfType<PlayerCarry>();
        if (!carry || !carry.IsCarrying)
        {
            Debug.LogWarning("Tu n'as rien à donner.");
            return;
        }

        GameObject carried = carry.GetCarriedObject();
        Cup cup = carried.GetComponent<Cup>();

        if (cup != null && cup.IsFull)
        {
            carry.RemoveCarried();
            ReceiveCommand(carried);
            CurrencyManager.Instance.AddCoins(10);
        }
        else
        {
            Debug.LogWarning("Ce n'est pas un café plein.");
        }
    }
    
    public void Collect() { }

    public void InitCommandSpot()
    {
        commandSpot = ClientBarSpotManager.Instance.RequestFreeSpot();
        if (commandSpot == null)
        {
            Debug.Log("Bar plein, le client repart.");
            clientController.clientSpawner.DespawnClient(gameObject);
            return;
        }
        
        clientController.movement.MoveTo(commandSpot.transform);
    }
    
    public void StartOrder()
    {
        _canInteract = true;
        _patienceCoroutine = StartCoroutine(PatienceTimer());
        Debug.Log("Un client veut un café !");
    }

    private void ReceiveCommand(GameObject commandObj)
    {
        StopCoroutine(_patienceCoroutine);
        
        _heldCup = commandObj;
        Transform cupSpot = ClientBarSpotManager.Instance.GetCupSpot(commandSpot.gameObject);
        _heldCup.GetComponent<FollowTarget>().SetTarget(cupSpot);
        _heldCup.GetComponent<Cup>().Lock();
        _heldCup.GetComponent<Cup>().OnSpot(commandSpot);

        StartCoroutine(DrinkCoffee());
    }

    private IEnumerator DrinkCoffee()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("Client a fini son café.");
        
        Cup heldCupScript = _heldCup.GetComponent<Cup>();
        heldCupScript.Empty();
        heldCupScript.Unlock();
        
        LeaveCoffeeShop();
    }
    
    public IEnumerator PatienceTimer()
    {
        yield return new WaitForSeconds(patienceTime);
        Debug.Log("Client impatient, il part.");
        ClientBarSpotManager.Instance.ReleaseSpot(commandSpot);
        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }

    public void LeaveCoffeeShop()
    {
        Transform exit = clientController.clientSpawner.GetRandomExit();
        clientController.movement.MoveTo(exit);
    }
}
