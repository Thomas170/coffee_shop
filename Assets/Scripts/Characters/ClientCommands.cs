using System.Collections;
using UnityEngine;

public class ClientCommands : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform handPoint;

    private readonly bool _hasOrdered = true;
    private bool _hasCup;
    private GameObject _heldCup;

    public void Interact()
    {
        PlayerCarry carry = FindObjectOfType<PlayerCarry>();
        if (!_hasOrdered || _hasCup)
        {
            Debug.LogWarning("Il n'y a pas de commandes en cours.");
            return;
        }
        
        if (!carry || !carry.IsCarrying)
        {
            Debug.LogWarning("Tu n'as pas d'objet en main.");
            return;
        }

        GameObject carried = carry.GetCarriedObject();
        Cup cup = carried.GetComponent<Cup>();

        if (cup != null && cup.IsFull)
        {
            ReceiveCup(cup.gameObject);
            carry.RemoveCarried();
            CurrencyManager.Instance.AddCoins(10);
        }
        else
        {
            Debug.LogWarning("Tu dois tenir en main une tasse de café.");
        }
    }

    private void ReceiveCup(GameObject cupObj)
    {
        _hasCup = true;
        _heldCup = cupObj;
        _heldCup.GetComponent<FollowTarget>().SetTarget(handPoint);

        StartCoroutine(DrinkCoffee());
    }
    
    private IEnumerator DrinkCoffee()
    {
        yield return new WaitForSeconds(20f);
        Debug.Log("Drunk coffee !");

        if (_heldCup)
        {
            Cup cup = _heldCup.GetComponent<Cup>();
            cup.Empty();
        }
    }

    public bool CanGiveCupBack()
    {
        if (_heldCup == null) return false;

        Cup cupScript = _heldCup.GetComponent<Cup>();
        return cupScript != null && !cupScript.IsFull;
    }

    public void Collect()
    {
        if (!CanGiveCupBack())
        {
            Debug.LogWarning("Tu ne peux pas encore récupérer la tasse.");
            return;
        }
        
        GameObject player = GameObject.FindWithTag("Player");
        var carry = player.GetComponent<PlayerCarry>();
        
        if (!carry.IsCarrying)
        {
            carry.PickUp(_heldCup);
            _heldCup = null;
            Debug.Log("Tasse vide récupérée !");
        }
        else
        {
            Debug.LogWarning("Pose ton objet avant !");
        }
    }
}
