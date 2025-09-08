using System.Collections;
using UnityEngine;

public class StartScene : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private GameObject[] carPrefabs;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float carSpeed = 100f;
    [SerializeField] private float spawnInterval = 10f;

    private int _currentCarIndex;
    private AudioSource _carAudioSource;
    private Coroutine _spawnCoroutine;

    private void Start()
    {
        _spawnCoroutine = StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            _carAudioSource = SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.car);

            SpawnCar();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCar()
    {
        if (carPrefabs.Length == 0) return;

        GameObject carPrefab = carPrefabs[_currentCarIndex];
        _currentCarIndex = (_currentCarIndex + 1) % carPrefabs.Length;

        GameObject car = Instantiate(carPrefab, pointA.position, pointA.rotation);
        StartCoroutine(MoveCar(car));
    }

    private IEnumerator MoveCar(GameObject car)
    {
        while (car && Vector3.Distance(car.transform.position, pointB.position) > 0.1f)
        {
            car.transform.position = Vector3.MoveTowards(
                car.transform.position,
                pointB.position,
                carSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (car)
            Destroy(car);
    }

    private void OnDestroy()
    {
        if (_carAudioSource)
        {
            SoundManager.Instance.StopSound(_carAudioSource);
        }

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
    }
}
