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
    public List<Marker> AllMarkers { get; private set; }
    private List<CivilianController> Civilians;

    private RoadsGenerator roadsGenerator;

    private CityGenerator cityGenerator;


    private void Awake()
    {
        roadsGenerator = GetComponent<RoadsGenerator>();
        cityGenerator = FindObjectOfType<CityGenerator>();
    }


    void Start()
    {
        currentlySpawned = 0;
        usedToSpawn = new List<Marker>();
        Civilians = new List<CivilianController>();
        AllMarkers = new List<Marker>();
        roadsGenerator.GenerateRoads();
        cityRoads = roadsGenerator.GetRoads();
        ConnectRoads();
        SpawnCivilianCars();
        if (cityGenerator) cityGenerator.Generate();
    }

    private void ConnectRoads()
    {
        foreach (Road road in cityRoads)
        {
            road.ConnectMarkers(cityRoads);
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
            MoveToWaypointBehavior moveToWaypointBehavior = civilianCar.GetComponent<MoveToWaypointBehavior>();
            moveToWaypointBehavior.SetTargetMarker(spawnMarker.GetNextAdjacentMarker());
            civilianCar.name = "Civilian_" + currentlySpawned;
            Civilians.Add(civilianCar.GetComponent<CivilianController>());

            currentlySpawned++;

            usedToSpawn.Add(spawnMarker);

            if (currentlySpawned >= numberToSpawn)
            {
                return;
            }
        }
    }

    private void Update()
    {
        if (AllMarkers.Count == 0)
            GetAllMarkers();
    }

    private void GetAllMarkers()
    {
        // Check if all roads are initialized
        bool areInitialized = true;
        for (int i = 0; i < cityRoads.Count && areInitialized; i++)
            areInitialized &= cityRoads[i].allMarkers != null && cityRoads[i].allMarkers.Count > 0;

        // If are initialized, store all of them
        if (areInitialized)
            foreach (Road road in cityRoads)
                AllMarkers.AddRange(road.allMarkers);
    }

    public bool IsMarkerAvailable(Marker mkr, List<GameObject> otherObjs = null, float maxDist = 10)
    {
        bool isAvailable = true;
        int i;

        // Check if some civilian is there
        for (i = 0; i < Civilians.Count && isAvailable; i++)
            isAvailable = (Civilians[i].transform.position - mkr.transform.position).magnitude > maxDist;

        // Check if additional civilians are there
        for (i = 0; i < otherObjs.Count && isAvailable; i++)
            isAvailable = (otherObjs[i].transform.position - mkr.transform.position).magnitude > maxDist;

        return isAvailable;
    }

    public Marker GetClosestMarker(Vector3 pos)
    {
        Marker marker = null;
        float minDist = float.PositiveInfinity;
        float dist;
        foreach (Marker mkr in AllMarkers)
        {
            dist = (pos - mkr.transform.position).magnitude;
            if (dist < minDist)
            {
                marker = mkr;
                minDist = dist;
            }
        }

        return marker;
    }

    public Marker GetRandomMarker(bool checkIfAvailable = false, List<GameObject> otherObjs = null)
    {
        Marker selectedMarker = null;

        int tries = 0;
        int idx;
        Marker mkr;
        bool isFound = false;
        while (tries < cityRoads.Count * 10 && !isFound)
        {
            idx = UnityEngine.Random.Range(0, cityRoads.Count);
            mkr = cityRoads[idx].GetPositionForCarToSpawn();

            // If invalid, increment number of tries
            if (mkr == null || (checkIfAvailable && !IsMarkerAvailable(mkr, otherObjs)))
                tries++;
            // If valid, set it
            else
            {
                selectedMarker = mkr;
                isFound = true;
            }
        }

        if (!isFound)
            Debug.LogWarning("None available road marker found");

        return selectedMarker;
    }

}
