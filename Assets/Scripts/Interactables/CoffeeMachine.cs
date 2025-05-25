using UnityEngine;
using System.Collections;

public class CoffeeMachine : MonoBehaviour//, IInteractable
{
    [SerializeField] private Transform cupPlacementPoint;
    [SerializeField] private CoffeeGaugeUI gaugeUI;
    private Cup _placedCup;
    private bool _isBrewing;

    /*public void Interact()
    {
        if (_isBrewing || _placedCup != null) return;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerCarry carry = player.GetComponentInChildren<PlayerCarry>();

        if (!carry.IsCarrying) {
            Debug.LogWarning("Il faut une tasse vide !");
            return;
        }

        GameObject carried = carry.GetCarriedObject();
        Cup cup = carried.GetComponent<Cup>();

        if (cup == null || cup.State != CupState.Empty) {
            Debug.LogWarning("Ce n'est pas une tasse vide !");
            return;
        }

        carry.RemoveCarried();
        _placedCup = cup;
        _placedCup.GetComponent<FollowTarget>().SetTarget(cupPlacementPoint);
        
        StartCoroutine(BrewCoffee());
        Debug.Log("Préparation du café...");
    }

    public void Collect()
    {
        if (_placedCup != null && _placedCup.State == CupState.Full)
        {
            GameObject player = GameObject.FindWithTag("Player");
            var carry = player.GetComponent<PlayerCarry>();

            if (!carry.IsCarrying)
            {
                carry.PickUp(_placedCup.gameObject);
                _placedCup = null;
                Debug.Log("Café récupéré !");
            }
            else
            {
                Debug.LogWarning("Pose ton objet avant !");
            }
        }
    }

    private IEnumerator BrewCoffee()
    {
        _isBrewing = true;
        
        gaugeUI.StartFilling(5f);
        yield return new WaitForSeconds(5f);
        gaugeUI.Hide();
        
        _placedCup.Fill();
        _placedCup.Unlock();
        _isBrewing = false;
        Debug.Log("Café prêt !");
    }*/
}