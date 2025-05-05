using UnityEngine;
using System.Collections;

public class CoffeeMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform cupPlacementPoint;
    private Cup _placedCup;
    private bool _isBrewing;

    public void Interact()
    {
        if (_isBrewing || _placedCup != null) return;

        GameObject player = GameObject.FindWithTag("Player");
        PlayerCarry carry = player.GetComponentInChildren<PlayerCarry>();

        if (!carry.IsCarrying) {
            Debug.Log("Il faut une tasse vide !");
            return;
        }

        GameObject carried = carry.GetCarriedObject();
        Cup cup = carried.GetComponent<Cup>();

        if (cup == null || cup.State != CupState.Empty) {
            Debug.Log("Ce n'est pas une tasse vide !");
            return;
        }

        carry.RemoveCarried(); // supprime la tasse dans la main
        _placedCup = Instantiate(cup.gameObject, cupPlacementPoint.position, Quaternion.identity).GetComponent<Cup>();
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
                Debug.Log("Pose ton objet avant !");
            }
        }
    }

    private IEnumerator BrewCoffee()
    {
        _isBrewing = true;
        yield return new WaitForSeconds(5f);
        _placedCup.Fill();
        _isBrewing = false;
        Debug.Log("Café prêt !");
    }
}