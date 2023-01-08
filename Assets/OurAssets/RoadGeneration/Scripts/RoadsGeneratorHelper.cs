using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadsGeneratorHelper : MonoBehaviour
{
    public GameObject straightRoadPrefab;
    public GameObject curveRoadPrefab;
    public GameObject threeWayRoadPrefab;
    public GameObject fourWayRoadPrefab;
    public GameObject deadEndRoadPrefab;

    private Dictionary<Vector3Int, GameObject> roadDictionary = new();
    private List<Vector3Int> fixRoadCandidates = new();
    private List<Vector3Int> deadEndCandidates = new();

    int numCreated = 0;
    
    public void PlaceRoads(Vector3 startPosition, Vector3 direction, int numToPlaceInBetween, float roadLength)
    {
        // We place the extremes and those in between
        int numToPlace = numToPlaceInBetween + 2;
        for (int i = 0; i < numToPlace; i++)
        {
            Vector3Int position = Vector3Int.RoundToInt(startPosition + direction * i * roadLength);

            // We avoid repeating an extreme already placed
            if (roadDictionary.ContainsKey(position))
            {
                continue;
            }

            PlaceInitialStraightRoad(position, direction);

            if (i == 0 || i == numToPlace - 1)
            {
                // We might have to fix the extremes, the others will be always straight roads
                fixRoadCandidates.Add(position);
            }
            if (i == numToPlace - 1)
            {
                deadEndCandidates.Add(position);
            }
        }
    }

    public void PlaceInitialStraightRoad(Vector3Int position, Vector3 direction)
    {
        Vector3Int directionInt = Vector3Int.RoundToInt(direction);
        Quaternion rotation = directionInt.x == 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
        GameObject road = Instantiate(straightRoadPrefab, position, rotation, transform);
        road.name = "Road_" + numCreated;
        numCreated += 1;
        roadDictionary.Add(position, road);
    }


    public void FixRoads(float roadLength)
    {
        foreach (Vector3Int roadPosition in fixRoadCandidates)
        {
            List<Vector3Int> neighbourRoads = GetNeighbourRoads(roadPosition, roadLength);

            if (neighbourRoads.Count == 2)
            {
                if (IsStraightRoad(roadPosition, neighbourRoads))
                {
                    PlaceStraightRoad(roadPosition, neighbourRoads);
                }
                else
                {
                    PlaceCurveRoad(roadPosition, neighbourRoads);
                }
                
            }
            else if (neighbourRoads.Count == 3)
            {
                PlaceThreeWayRoad(roadPosition, neighbourRoads);
            }

            else if (neighbourRoads.Count == 4)
            {
                PlaceFourWayRoad(roadPosition, neighbourRoads);
            }
        }
    }

    public void FixDeadEnds(float roadLength)
    {
        foreach (Vector3Int roadPosition in deadEndCandidates)
        {
            List<Vector3Int> neighbourRoads = GetNeighbourRoads(roadPosition, roadLength);
            if (neighbourRoads.Count == 1)
            {
                PlaceDeadEndRoad(roadPosition, neighbourRoads[0]);
            }
        }
    }

    private List<Vector3Int> GetNeighbourRoads(Vector3Int position, float roadLength)
    {
        List<Vector3Int> neighourPositions = new();
        AddNeighbourRoadPosition(position, neighourPositions, roadLength, Vector3.forward);
        AddNeighbourRoadPosition(position, neighourPositions, roadLength, -Vector3.forward);
        AddNeighbourRoadPosition(position, neighourPositions, roadLength, Vector3.right);
        AddNeighbourRoadPosition(position, neighourPositions, roadLength, -Vector3.right);
        return neighourPositions;
    }

    private void AddNeighbourRoadPosition(Vector3Int position, List<Vector3Int> neighourPositions, float roadLength, Vector3 direction)
    {
        Vector3Int newPosition = Vector3Int.RoundToInt(position + direction * roadLength);
        if (roadDictionary.ContainsKey(newPosition))
        {
            neighourPositions.Add(newPosition);
        }
    }

    private bool IsStraightRoad(Vector3Int roadPosition, List<Vector3Int> neighbourRoads)
    {
        bool sameX = roadPosition.x == neighbourRoads[0].x && roadPosition.x == neighbourRoads[1].x;
        bool sameY = roadPosition.z == neighbourRoads[0].z && roadPosition.z == neighbourRoads[1].z;
        return sameX || sameY;
    }

    private void PlaceStraightRoad(Vector3Int roadPosition, List<Vector3Int> neighbourPositions)
    {
        Quaternion rotation = roadPosition.x == neighbourPositions[0].x ? 
            Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 90, 0);
        Destroy(roadDictionary[roadPosition]);
        GameObject straightRoad = Instantiate(straightRoadPrefab, roadPosition, rotation, transform);
        roadDictionary[roadPosition] = straightRoad;
        straightRoad.name = "Road_" + numCreated;
        numCreated += 1;
    }

    private void PlaceCurveRoad(Vector3Int roadPosition, List<Vector3Int> neighbourPositions)
    {
        Quaternion rotation;
        if (roadPosition.x < neighbourPositions[1].x && roadPosition.z < neighbourPositions[0].z)
        {
            rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (roadPosition.x > neighbourPositions[1].x && roadPosition.z < neighbourPositions[0].z)
        {
            rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (roadPosition.x < neighbourPositions[1].x && roadPosition.z > neighbourPositions[0].z)
        {
            rotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            rotation = Quaternion.Euler(0, -90, 0);
        }

        Destroy(roadDictionary[roadPosition]);
        GameObject curveRoad = Instantiate(curveRoadPrefab, roadPosition, rotation, transform);
        roadDictionary[roadPosition] = curveRoad;
        curveRoad.name = "Road_" + numCreated;
        numCreated += 1;

    }

    private void PlaceThreeWayRoad(Vector3Int roadPosition, List<Vector3Int> neighbourPositions)
    {
        Vector3Int otherRoadPosition = GetThreeWayPerpendicularNeighbour(roadPosition, neighbourPositions);
        Quaternion rotation;

        if (roadPosition.x == otherRoadPosition.x)
        {
            if (roadPosition.z > otherRoadPosition.z)
            {
                rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, -90, 0);
            }
        }
        else
        {
            if (roadPosition.x > otherRoadPosition.x)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        Destroy(roadDictionary[roadPosition]);
        GameObject threeWayRoad = Instantiate(threeWayRoadPrefab, roadPosition, rotation, transform);
        roadDictionary[roadPosition] = threeWayRoad;
        threeWayRoad.name = "Road_" + numCreated;
        numCreated += 1;
    }

    private Vector3Int GetThreeWayPerpendicularNeighbour(Vector3Int roadPosition, List<Vector3Int> neighbourPositions)
    {
        // The road that goes perpendicular to the others
        List<int> sameXIndices = new();
        List<int> sameZIndices = new();

        for (int i = 0; i < neighbourPositions.Count; ++i)
        {
            if (neighbourPositions[i].x == roadPosition.x)
            {
                sameXIndices.Add(i + 1);
            }
            else if (neighbourPositions[i].z == roadPosition.z)
            {
                sameZIndices.Add(i + 1);
            }
        }

        if (sameXIndices.Count == 2)
        {
            // Get which position is missing
            int index = 6 - sameXIndices.Sum();
            return neighbourPositions[index - 1];
        }
        else
        {
            int index = 6 - sameZIndices.Sum();
            return neighbourPositions[index - 1];
        }
    }

    private void PlaceFourWayRoad(Vector3Int roadPosition, List<Vector3Int> neighbourPositions)
    {
        Destroy(roadDictionary[roadPosition]);
        GameObject fourWayRoad = Instantiate(fourWayRoadPrefab, roadPosition, Quaternion.Euler(0, 0, 0), transform);
        roadDictionary[roadPosition] = fourWayRoad;
        fourWayRoad.name = "Road_" + numCreated;
        numCreated += 1;
    }


    private void PlaceDeadEndRoad(Vector3Int roadPosition, Vector3Int neighbourPosition)
    {
        Destroy(roadDictionary[roadPosition]);
        Quaternion rotation = ComputeDeadEndRotation(roadPosition, neighbourPosition);
        GameObject deadEndRoad = Instantiate(deadEndRoadPrefab, roadPosition, rotation, transform);
        roadDictionary[roadPosition] = deadEndRoad;
        deadEndRoad.name = "Road_" + numCreated;
        numCreated += 1;
    }

    private Quaternion ComputeDeadEndRotation(Vector3Int roadPosition, Vector3Int neighbourPosition)
    {
        if (roadPosition.x > neighbourPosition.x)
        {
            return Quaternion.Euler(0, 90, 0);
        }
        else if (roadPosition.x < neighbourPosition.x)
        {
            return Quaternion.Euler(0, -90, 0);
        }
        else if (roadPosition.z > neighbourPosition.z)
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else
        {
            return Quaternion.Euler(0, 180, 0);
        }
    }

    public List<Vector3Int> GetRoadPositions()
    {
        return roadDictionary.Keys.ToList();
    }

    public List<Road> GetRoads()
    {
        List<Road> roads = new();
        foreach (GameObject roadGameObject in roadDictionary.Values)
        {
            Road road = roadGameObject.GetComponent<Road>();
            roads.Add(road);
        }
        return roads;
    }

}
