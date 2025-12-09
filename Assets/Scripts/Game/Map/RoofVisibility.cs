using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class RoofVisibility : NetworkBehaviour
{
    [SerializeField] private Renderer[] roofRenderers;
    [SerializeField] private float fadeDuration = 0.5f;

    private List<Material> _materials = new List<Material>();
    private Tween _currentTween;
    private int _roomsInsideCount = 0;

    private void Start()
    {
        foreach (var r in roofRenderers)
        {
            r.gameObject.SetActive(true);

            // Instance du material (sinon sharedMaterial = fade pour tous les joueurs !)
            Material mat = r.material;  

            _materials.Add(mat);

            SetAlpha(mat, 1f);
        }
    }

    public void PlayerEnteredRoom(Collider other)
    {
        if (!IsLocalPlayer(other)) return;

        _roomsInsideCount++;
        if (_roomsInsideCount == 1)
            StartCoroutine(FadeTo(0f));
    }

    public void PlayerExitedRoom(Collider other)
    {
        if (!IsLocalPlayer(other)) return;

        _roomsInsideCount--;
        if (_roomsInsideCount <= 0)
        {
            _roomsInsideCount = 0;
            StartCoroutine(FadeTo(1f));
        }
    }

    private bool IsLocalPlayer(Collider other)
    {
        if (!other.CompareTag("Player")) return false;

        var netObj = other.GetComponent<NetworkObject>();
        return netObj && netObj.IsOwner && IsClient;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        _currentTween?.Kill();

        // Tween sur TOUS les matériaux
        _currentTween = DOTween.To(
            () => _materials[0].color.a,
            a => { foreach (var m in _materials) SetAlpha(m, a); },
            targetAlpha,
            fadeDuration
        );

        yield return new WaitForSeconds(fadeDuration);

        bool active = targetAlpha > 0.99f;
        foreach (var r in roofRenderers)
            r.gameObject.SetActive(active);
    }

    private void SetAlpha(Material mat, float alpha)
    {
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}
