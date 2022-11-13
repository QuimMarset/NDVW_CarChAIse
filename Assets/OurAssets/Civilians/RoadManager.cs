using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{

    [SerializeField]
    private int numberToSpawn;
    [SerializeField]
    private int currentlySpawned;
    [SerializeField]
    private CarSpawner carSpawner;

    private List<Road> cityRoads;
    private List<Marker> usedToSpawn;


    // Start is called before the first frame update
    void Start()
    {
        currentlySpawned = 0;
        cityRoads = new List<Road>();
        usedToSpawn = new List<Marker>();
        InitializeCityRoads();
        ConnectRoads();
        SpawnCivilianCars();
    }

    private void ConnectRoads()
    {
        foreach (Road road in cityRoads)
        {
            road.ConnectMarkers(cityRoads);
        }
    }

    private void InitializeCityRoads()
    {
        int numOfChilds = gameObject.transform.childCount;
        for (int i = 0; i < numOfChilds; ++i)
        {
            Road cityRoad = gameObject.transform.GetChild(i).GetComponent<Road>();
            cityRoads.Add(cityRoad);
        }
    }
 
    private bool CheckIfMarkerAlreadyUsed(Marker candidateMarker)
    {
        foreach (Marker marker in usedToSpawn)
        {
            if (candidateMarker.Equals(marker))
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnCivilianCars()
    {
        foreach (Road cityRoad in cityRoads)
        {
            Marker spawnMarker = cityRoad.GetPositionForCarToSpawn();
            
            if (spawnMarker == null || CheckIfMarkerAlreadyUsed(spawnMarker))
            {
                continue;
            }

            GameObject civilianCar = carSpawner.InstantiateCarPrefab(spawnMarker.transform.position, spawnMarker.transform.rotation);
            CivilianAI civilianAI = civilianCar.GetComponent<CivilianAI>();
            civilianAI.SetTargetMarker(spawnMarker.GetNextAdjacentMarker());
            
            currentlySpawned++;
            
            usedToSpawn.Add(spawnMarker);

            if (currentlySpawned >= numberToSpawn)
            {
                return;
            }
        }
    }
}
