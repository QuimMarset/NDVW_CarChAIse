using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{

    public List<GameObject> carPrefabs;

    public GameObject InstantiateCarPrefab(Vector3 position, Quaternion rotation)
    {
        GameObject carPrefab = GetRandomCarPrefab();
        GameObject car = Instantiate(carPrefab, position, rotation);
        return car;
    }

    private GameObject GetRandomCarPrefab()
    {
        int index = Random.Range(0, carPrefabs.Count);
        return carPrefabs[index];
    }
}
