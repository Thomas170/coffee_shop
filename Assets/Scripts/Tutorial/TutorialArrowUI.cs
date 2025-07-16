using UnityEngine;
using UnityEngine.UI;

public class TutorialArrowUI : MonoBehaviour
{
    public Transform target; // cible en 3D
    public Camera mainCamera;
    public RectTransform canvasRect;

    private RectTransform arrowRect;

    private void Awake()
    {
        arrowRect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!target) return;

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position + Vector3.up * 1.5f);

        bool isBehind = screenPoint.z < 0;
        if (isBehind) screenPoint *= -1;

        Vector2 clampedPos = screenPoint;
        clampedPos.x = Mathf.Clamp(clampedPos.x, 50, Screen.width - 50);
        clampedPos.y = Mathf.Clamp(clampedPos.y, 50, Screen.height - 50);

        arrowRect.position = clampedPos;

        Vector3 dir = (screenPoint - new Vector3(Screen.width / 2, Screen.height / 2, 0)).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowRect.rotation = Quaternion.Euler(0, 0, angle - 90);
    }
}