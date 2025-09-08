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

    private int _currentCarIndex = 0;

    private void Start()
    {
        StartCoroutine(SpawnCarsRoutine());
    }

    private IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            SoundManager.Instance.PlayGlobalSound(SoundManager.Instance.car);
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
}