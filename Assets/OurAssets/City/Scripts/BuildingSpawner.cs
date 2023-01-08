using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSpawner : MonoBehaviour
{
    public GameObject buildingPrefab;

    public RoadsGenerator roadsGenerator;

    public void Generate()
    {
        SpawnBuildings();
    }

    void SpawnBuildings()
    {
        List<Vector3Int> spawnPoints = roadsGenerator.GetPositionsToSpawnBuildings();
        Debug.Log(spawnPoints.Count);

        foreach (Vector3Int position in spawnPoints)
        {
            GameObject obj = Instantiate(buildingPrefab, position, transform.rotation);
            obj.transform.SetParent(this.transform);

            CityGenerator.instance.AddObject(obj);
        }
    }
}
