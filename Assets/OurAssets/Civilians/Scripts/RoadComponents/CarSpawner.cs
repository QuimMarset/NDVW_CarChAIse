using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{

    public List<GameObject> carPrefabs;

    public GameObject InstantiateCarPrefab(Vector3 position, Quaternion rotation)
    {
        int index = Random.Range(0, carPrefabs.Count);
        GameObject carPrefab = carPrefabs[index];
        GameObject car = Instantiate(carPrefab, position, rotation);
        if (CanBePainted(index))
        {
            ChangeColor(car);
        }
        return car;
    }

    private bool CanBePainted(int index)
    {   
        // 0 corresponds to the taxi
        return index >= 0;
    }
    
    private void ChangeColor(GameObject car)
    {
        MeshRenderer meshRenderer = car.GetComponentInChildren<MeshRenderer>();
        Color color = UnityEngine.Random.ColorHSV();
        meshRenderer.material.SetColor("_Color", color);
    }
}
