using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private BaseMenuController _menuController;
    private int _index;
    private RectTransform _rectTransform;
    private Button _button;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // Vérifier si la souris est déjà dessus quand le bouton devient actif
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            StartCoroutine(CheckMousePositionDelayed());
        }
    }

    public void Init(BaseMenuController controller, int index)
    {
        _menuController = controller;
        _index = index;
    }

    private System.Collections.IEnumerator CheckMousePositionDelayed()
    {
        // Attendre que le layout soit stabilisé
        yield return new WaitForEndOfFrame();
        yield return null;
        
        CheckIfMouseIsOver();
    }

    private void CheckIfMouseIsOver()
    {
        if (_button == null || !_button.interactable || !_button.gameObject.activeInHierarchy)
            return;

        if (_menuController == null || InputDeviceTracker.Instance.IsUsingGamepad)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, mousePosition, null))
        {
            _menuController.SelectButton(_index);
            
            // Simuler l'événement pour EventSystem
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };
            ExecuteEvents.Execute(gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            _menuController?.SelectButton(_index);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!InputDeviceTracker.Instance.IsUsingGamepad)
        {
            EventSystem.current?.SetSelectedGameObject(null);
            _menuController?.ClearSelection();
        }
    }
}