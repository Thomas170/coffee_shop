using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHighlight : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image background;
    private MenuController _menuController;
    private int _myIndex;

    public void Init(MenuController controller, int index)
    {
        _menuController = controller;
        _myIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_menuController != null)
            _menuController.SelectButton(_myIndex);
    }
}