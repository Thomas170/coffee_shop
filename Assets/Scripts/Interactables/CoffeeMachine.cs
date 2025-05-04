using UnityEngine;
using System.Collections;

public class CoffeeMachine : MonoBehaviour, IInteractable
{
    private bool _isBrewing;
    private bool _coffeeReady;

    public void Interact()
    {
        if (!_isBrewing && !_coffeeReady)
        {
            Debug.Log("Préparation du café...");
            StartCoroutine(BrewCoffee());
        }
        else if (_coffeeReady)
        {
            Debug.Log("Café déjà prêt, récupérez-le.");
        }
        else
        {
            Debug.Log("Déjà en préparation...");
        }
    }

    public void Collect()
    {
        if (_coffeeReady)
        {
            _coffeeReady = false;
            Debug.Log("Café récupéré !");
        }
        else
        {
            Debug.Log("Aucun café à récupérer.");
        }
    }

    private IEnumerator BrewCoffee()
    {
        _isBrewing = true;
        yield return new WaitForSeconds(5f);
        _isBrewing = false;
        _coffeeReady = true;
        Debug.Log("Café prêt !");
    }
}