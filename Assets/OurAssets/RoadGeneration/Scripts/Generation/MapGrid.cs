using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum GridPosition
{
    Invalid,
    Empty,
    Road,
    Building
}

public class MapGrid : MonoBehaviour
{

    private Dictionary<Road, List<Road>> roadAdjacency;

    private Vector3Int topLeftCorner;
    private Vector3Int topRightCorner;
    private Vector3Int bottomLeftCorner;
    private Vector3Int bottomRightCorner;
    private int width;
    private int height;

    private Dictionary<Vector3Int, GridPosition> gridOccupancy = new();

    private void ComputeGridLimits(List<Vector3Int> roadPositions, int scaleFactor, int roadLength)
    {
        int minX = int.MaxValue;
        int minZ = int.MaxValue;
        int maxX = int.MinValue;
        int maxZ = int.MinValue;

        for (int i = 0; i < roadPositions.Count; ++i)
        {
            Vector3Int roadPosition = roadPositions[i] * scaleFactor;
            minX = roadPosition.x < minX ? roadPosition.x : minX;
            minZ = roadPosition.z < minZ ? roadPosition.z : minZ;
            maxX = roadPosition.x > maxX ? roadPosition.x : maxX;
            maxZ = roadPosition.z > maxZ ? roadPosition.z : maxZ;
        }

        int extraCellSpace = scaleFactor * roadLength;

        topLeftCorner = new(maxX + extraCellSpace, 0, minZ - extraCellSpace);
        topRightCorner = new(minX - extraCellSpace, 0, minZ - extraCellSpace);
        bottomLeftCorner = new(maxX + extraCellSpace, 0, maxZ + extraCellSpace);
        bottomRightCorner = new(minX - extraCellSpace, 0, maxZ + extraCellSpace);
    }

    private void FillGridWithRoads(List<Vector3Int> roadPositions, int scaleFactor)
    {
        foreach (Vector3Int roadPosition in roadPositions)
        {
            gridOccupancy[roadPosition * scaleFactor] = GridPosition.Road;
        }
    }

    private void FillGridEmptySpaces(int roadLength, int scaleFactor)
    {
        width = (topLeftCorner.x - topRightCorner.x) / (scaleFactor * roadLength) + 1;
        height = (bottomLeftCorner.z - topLeftCorner.z + 1) / (scaleFactor * roadLength) + 1;

        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int x = topLeftCorner.x - roadLength * j * scaleFactor;
                int z = topLeftCorner.z + roadLength * i * scaleFactor;
                Vector3Int position = new(x, 0, z);
                if (!gridOccupancy.ContainsKey(position))
                {
                    if (IsValidToPlaceBuilding(position, roadLength, scaleFactor))
                    {
                        gridOccupancy[position] = GridPosition.Empty;
                    }
                    else
                    {
                        gridOccupancy[position] = GridPosition.Invalid;
                    }
                }
            }
        }
    }

    private void FixFilteredEmptySpaces(int roadLength, int scaleFactor)
    {
        for (int i = 0; i < height; ++i)
        {
            for (int j = 0; j < width; ++j)
            {
                int x = topLeftCorner.x - roadLength * j * scaleFactor;
                int z = topLeftCorner.z + roadLength * i * scaleFactor;
                Vector3Int position = new(x, 0, z);
                if (gridOccupancy[position] == GridPosition.Invalid && 
                    IsInvalidPositionUsable(position, roadLength, scaleFactor))
                {
                    gridOccupancy[position] = GridPosition.Empty;
                }
            }
        }
    }

    private bool IsInvalidPositionUsable(Vector3Int position, int roadLength, int scaleFactor)
    {
        bool isUpValid = CheckIfNeighbourIsType(position, Vector3Int.forward, roadLength, scaleFactor, GridPosition.Empty);
        bool isDownValid = CheckIfNeighbourIsType(position, -Vector3Int.forward, roadLength, scaleFactor, GridPosition.Empty);
        bool isRightValid = CheckIfNeighbourIsType(position, Vector3Int.right, roadLength, scaleFactor, GridPosition.Empty);
        bool isLeftValid = CheckIfNeighbourIsType(position, -Vector3Int.right, roadLength, scaleFactor, GridPosition.Empty);
        int numEmpty = isUpValid ? 1 : 0;
        numEmpty += isDownValid ? 1 : 0;
        numEmpty += isRightValid ? 1 : 0;
        numEmpty += isLeftValid ? 1 : 0;
        return numEmpty >= 3;
    }

    private bool IsValidToPlaceBuilding(Vector3Int position, int roadLength, int scaleFactor)
    {
        bool isUpValid = CheckIfNeighbourIsType(position, Vector3Int.forward, roadLength, scaleFactor, GridPosition.Road);
        bool isDownValid = CheckIfNeighbourIsType(position, -Vector3Int.forward, roadLength, scaleFactor, GridPosition.Road);
        bool isRightValid = CheckIfNeighbourIsType(position, Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        bool isLeftValid = CheckIfNeighbourIsType(position, -Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        
        bool isLeftUpValid = CheckIfNeighbourIsType(position, Vector3Int.forward - Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        bool isRightUpValid = CheckIfNeighbourIsType(position, Vector3Int.forward + Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        bool isLeftBottomValid = CheckIfNeighbourIsType(position, -Vector3Int.forward - Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        bool isRightBottomValid = CheckIfNeighbourIsType(position, -Vector3Int.forward + Vector3Int.right, roadLength, scaleFactor, GridPosition.Road);
        
        return isUpValid || isDownValid || isRightValid || isLeftValid || isLeftUpValid || isRightUpValid || isLeftBottomValid || isRightBottomValid;
    }

    private bool CheckIfNeighbourIsType(Vector3Int position, Vector3Int direction, int roadLength, int scaleFactor, GridPosition positionType)
    {
        Vector3Int newPosition = position + direction * roadLength * scaleFactor;
        if (gridOccupancy.ContainsKey(newPosition))
        {
            GridPosition gridPosition = gridOccupancy[newPosition];
            return gridPosition == positionType;
        }
        return false;
    }

    public void BuildGrid(List<Vector3Int> roadPositions, int roadLength, int scaleFactor)
    {
        ComputeGridLimits(roadPositions, scaleFactor, roadLength);
        FillGridWithRoads(roadPositions, scaleFactor);
        FillGridEmptySpaces(roadLength, scaleFactor);
        FixFilteredEmptySpaces(roadLength, scaleFactor);
    }

    private bool IsPositionEmpty(Vector3Int position)
    {
        if (gridOccupancy.ContainsKey(position))
        {
            return gridOccupancy[position] == GridPosition.Empty;
        }
        return false;
    }

    public List<Vector3Int> GetEmptyPositions()
    {
        List<Vector3Int> emptyPositions = new();
        foreach (Vector3Int position in gridOccupancy.Keys)
        {
            if (IsPositionEmpty(position))
            {
                emptyPositions.Add(position);
            }
        }
        return emptyPositions;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (Vector3Int position in gridOccupancy.Keys)
        {
            if (gridOccupancy[position] == GridPosition.Empty)
            {
                Gizmos.color = Color.yellow;
            }
            else if (gridOccupancy[position] == GridPosition.Road)
            {
                Gizmos.color = Color.blue;
            }
            else if (gridOccupancy[position] == GridPosition.Invalid)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawSphere(position, 2);
        }
    }
    
}
