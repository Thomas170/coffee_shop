using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using Unity.Netcode;

public class IntroCinematic : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCamera cinematicCam;
    private CinemachineVirtualCamera _playerCam;

    [Header("UI")]
    [SerializeField] private CanvasGroup gameTitle;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float titleStayTime = 2f;
    [SerializeField] private GameObject level;
    [SerializeField] private GameObject coins;

    [Header("Cinematic")]
    [SerializeField] private Transform camStartPos;
    [SerializeField] private Transform camEndPos;
    [SerializeField] private float camMoveDuration = 6f;

    private void Start()
    {
        FindObjectOfType<CinematicBars>().ShowBars();
        coins.SetActive(false);
        level.SetActive(false);
        
        cinematicCam.Priority = 10;
        StartCoroutine(PlayCinematic());
    }

    private IEnumerator PlayCinematic()
    {
        // Mettre la caméra au point de départ
        if (camStartPos && camEndPos)
        {
            cinematicCam.transform.position = camStartPos.position;
            cinematicCam.transform.rotation = camStartPos.rotation;
        }

        // Fade in du titre
        yield return new WaitForSeconds(0.5f);
        gameTitle.DOFade(1, fadeDuration);
        yield return new WaitForSeconds(titleStayTime);

        // Fade out
        gameTitle.DOFade(0, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        // Mouvement de caméra
        if (camEndPos)
        {
            cinematicCam.transform.DOMove(camEndPos.position, camMoveDuration).SetEase(Ease.InOutQuad);
            cinematicCam.transform.DORotateQuaternion(camEndPos.rotation, camMoveDuration).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(camMoveDuration);
        }

        // Récupérer la caméra du joueur local (elle doit être sur son prefab)
        var localPlayer = PlayerListManager.Instance.GetPlayer(NetworkManager.Singleton.LocalClientId);
        if (localPlayer)
        {
            _playerCam = localPlayer.GetComponentInChildren<CinemachineVirtualCamera>(true);

            if (_playerCam)
            {
                // Switch de caméra
                cinematicCam.Priority = 0;
                _playerCam.Priority = 10;
            }

            // Réactiver contrôle joueur
            localPlayer.playerBuild.enabled = true;
        }

        FindObjectOfType<CinematicBars>().HideBars();
        yield return new WaitForSeconds(2f);
        
        // Lancer le tuto
        TutorialManager.Instance.StartTutorial();
        coins.SetActive(true);
        level.SetActive(true);
    }
}