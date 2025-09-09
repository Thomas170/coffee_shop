using System.Collections;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class RoofVisibility : NetworkBehaviour
{
    [SerializeField] private Renderer roofRenderer;
    [SerializeField] private float fadeDuration = 0.5f;

    private Material _roofMaterial;
    private Tween _currentTween;

    private void Start()
    {
        roofRenderer.gameObject.SetActive(true);
            
        if (roofRenderer != null)
        {
            _roofMaterial = roofRenderer.material;
            SetAlpha(1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsLocalPlayerInside(other))
        {
            StartCoroutine(FadeTo(0f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLocalPlayerInside(other))
        {
            StartCoroutine(FadeTo(1f));
        }
    }

    private bool IsLocalPlayerInside(Collider other)
    {
        if (!other.CompareTag("Player")) return false;

        var networkObj = other.GetComponent<NetworkObject>();
        return networkObj != null && networkObj.IsOwner && IsClient;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (_roofMaterial == null) yield return null;

        _currentTween?.Kill();

        _currentTween = DOTween.To(
            () => _roofMaterial.color.a,
            a => SetAlpha(a),
            targetAlpha,
            fadeDuration
        );

        yield return new WaitForSeconds(fadeDuration);
        roofRenderer.gameObject.SetActive(Mathf.Approximately(targetAlpha, 1));
    }

    private void SetAlpha(float alpha)
    {
        if (_roofMaterial == null) return;

        Color c = _roofMaterial.color;
        c.a = alpha;
        _roofMaterial.color = c;
    }
}