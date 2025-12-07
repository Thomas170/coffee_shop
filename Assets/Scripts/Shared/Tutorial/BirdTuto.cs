using UnityEngine;
using Unity.Netcode;

public class BirdTuto : NetworkBehaviour
{
    [Header("Flight Settings")]
    public Vector3 startPosition = new(57f, 95f, -100f);
    public float flightDistance = 100f;
    public float flightSpeed = 23f;
    public float waveFrequency = 5f;
    public float animationDuration = 5f;

    [Header("Wave Pattern")]
    [Tooltip("Amplitudes successives pour les différentes phases du vol")]
    public float[] waveAmplitudes = { 2f, 4f, 1f };

    private bool _isFlying;
    private float _elapsedTime;
    private int _currentWaveIndex;
    private float _phaseDuration;
    private Vector3 _initialPos;

    private void Start()
    {
        transform.position = startPosition;
        _phaseDuration = animationDuration / waveAmplitudes.Length;
    }

    private void Update()
    {
        if (!_isFlying) return;

        _elapsedTime += Time.deltaTime;

        // Déplacement horizontal vers la gauche
        float moveX = -flightSpeed * Time.deltaTime;
        transform.position += new Vector3(moveX, 0f, 0f);

        // Détermination de la phase actuelle
        _currentWaveIndex = Mathf.Min((int)(_elapsedTime / _phaseDuration), waveAmplitudes.Length - 1);

        // Oscillation verticale avec amplitude dynamique
        float amplitude = waveAmplitudes[_currentWaveIndex];
        float newY = startPosition.y + Mathf.Sin(_elapsedTime * waveFrequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Fin de vol
        if (_elapsedTime >= animationDuration || transform.position.x <= startPosition.x - flightDistance)
        {
            _isFlying = false;
            gameObject.SetActive(false);
        }
    }

    // Lancer le vol depuis le serveur pour tout le monde
    [ServerRpc(RequireOwnership = false)]
    public void PlayBirdTutoServerRpc()
    {
        PlayBirdTutoClientRpc();
    }

    [ClientRpc]
    private void PlayBirdTutoClientRpc()
    {
        PlayBirdTuto();
    }

    public void PlayBirdTuto()
    {
        transform.position = startPosition;
        _elapsedTime = 0f;
        _isFlying = true;
        _currentWaveIndex = 0;
        gameObject.SetActive(true);
    }
}
