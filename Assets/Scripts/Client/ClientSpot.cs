using UnityEngine;

[ExecuteAlways]
public class ClientSpot : MonoBehaviour
{
    public GameObject buildParent;
    public int index;

    private void OnValidate()
    {
        UpdateIndex();
    }

    private void Awake()
    {
        UpdateIndex();
    }

    private void UpdateIndex()
    {
        if (buildParent == null)
        {
            buildParent = FindBuildParent(transform);
        }

        if (buildParent == null)
        {
            index = -1;
            return;
        }

        ClientSpot[] allSpots = buildParent.GetComponentsInChildren<ClientSpot>(true);

        for (int i = 0; i < allSpots.Length; i++)
        {
            if (allSpots[i] == this)
            {
                index = i;
                return;
            }
        }

        index = -1;
    }

    private GameObject FindBuildParent(Transform current)
    {
        while (current.parent != null)
        {
            if (current.parent.CompareTag("Spot"))
            {
                return current.parent.gameObject;
            }

            current = current.parent;
        }

        return null;
    }
}