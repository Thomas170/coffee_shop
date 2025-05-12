using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHighlight : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image background;
    [SerializeField] private MainMenuController menuController;
    private int _myIndex;

    public void Init(MainMenuController controller, int index)
    {
        menuController = controller;
        _myIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (menuController != null)
            menuController.SelectButton(_myIndex);
    }
}