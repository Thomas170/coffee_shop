using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHighlight : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image background;
    private IMenuEntryActionHandler _menuEntryActionHandler;
    private int _myIndex;

    public void Init(IMenuEntryActionHandler controller, int index)
    {
        _menuEntryActionHandler = controller;
        _myIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_menuEntryActionHandler != null)
            _menuEntryActionHandler.SelectButton(_myIndex);
    }
}