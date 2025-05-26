using Unity.Netcode;
using UnityEngine;

public class CoffeeMachine : InteractableBase
{
    [SerializeField] private GameObject fullCupPrefab;

    protected override void OnActionComplete()
    {
        if (!currentItem) return;

        var oldCup = currentItem.GetComponent<ItemBase>();
        if (oldCup)
        {
            Destroy(oldCup.gameObject);
        }

        GameObject fullCup = Instantiate(fullCupPrefab, itemDisplay.position, Quaternion.identity);
        fullCup.GetComponent<NetworkObject>().Spawn();

        currentItem = fullCup.GetComponent<ItemBase>();
        currentItem.AttachTo(itemDisplay);
    }

    protected override void OnForcedEnd()
    {
        if (currentItem != null)
        {
            GameObject player = GetLocalPlayer();
            var carry = player.GetComponent<PlayerCarry>();

            if (!carry.IsCarrying)
            {
                carry.TryPickUp(currentItem.gameObject);
                currentItem = null;
                isInUse.Value = false;
                gaugeUI.Hide();
            }
        }
    }
}