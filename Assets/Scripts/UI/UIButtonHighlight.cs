using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHighlight : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Image background;
    private BaseMenuController _baseMenuController;
    private int _myIndex;

    public void Init(BaseMenuController controller, int index)
    {
        _baseMenuController = controller;
        _myIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_baseMenuController != null)
            _baseMenuController.SelectButton(_myIndex);
    }
}