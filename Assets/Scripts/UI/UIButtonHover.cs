using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BaseMenuController _menuController;
    private int _index;

    public void Init(BaseMenuController controller, int index)
    {
        _menuController = controller;
        _index = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Si on utilise la souris → on sélectionne visuellement le bouton
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            _menuController.SelectButton(_index);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Si on utilise la souris → on désélectionne tout
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            EventSystem.current.SetSelectedGameObject(null);
            _menuController.ClearSelection();
        }
    }
}