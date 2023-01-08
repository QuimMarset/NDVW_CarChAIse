using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EncodingLetters
{
    unknown = '1',
    save = '[',
    load = ']',
    draw = 'F',
    turnRight = '+',
    turnLeft = '-'
}

public class RoadsGenerator : MonoBehaviour
{
    private LSystemGenerator lSystem;
    private RoadsGeneratorHelper roadHelper;
    private MapGrid mapGrid;

    [SerializeField]
    [Range(1, 5)]
    private int numOfRoadsInBetween;
    [SerializeField]
    private float roadLength;
    [SerializeField]
    private float scaleFactor;
    private float angle = 90;

    private Stack<AgentParameters> savedPoints = new();
    private Vector3 currentPosition = Vector3.zero;
    private Vector3 direction = Vector3.forward;
    private Vector3 tempPosition = Vector3.zero;

    private void Awake()
    {
        lSystem = GetComponent<LSystemGenerator>();
        roadHelper = GetComponent<RoadsGeneratorHelper>();
        mapGrid = GetComponent<MapGrid>();
    }

    private void Start()
    {
        // GenerateRoads();
    }

    public void GenerateRoads()
    {
        string finalSentence = lSystem.finalSentence;

        foreach (char letter in finalSentence)
        {
            EncodingLetters encodedLetter = (EncodingLetters)letter;

            switch (encodedLetter)
            {
                case EncodingLetters.save:

                    savedPoints.Push(new AgentParameters
                    {
                        position = currentPosition,
                        direction = direction
                    });

                    break;

                case EncodingLetters.load:

                    if (savedPoints.Count > 0)
                    {
                        AgentParameters agentParameter = savedPoints.Pop();
                        currentPosition = agentParameter.position;
                        direction = agentParameter.direction;
                    }

                    break;

                case EncodingLetters.draw:

                    tempPosition = currentPosition;
                    roadHelper.PlaceRoads(tempPosition, direction, numOfRoadsInBetween, roadLength);
                    currentPosition += direction * (numOfRoadsInBetween + 1) * roadLength;
                    break;

                case EncodingLetters.turnRight:

                    direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;

                    break;

                case EncodingLetters.turnLeft:

                    direction = Quaternion.AngleAxis(-angle, Vector3.up) * direction;

                    break;

                default:
                    break;
            }
        }

        roadHelper.FixRoads(roadLength);
        roadHelper.FixDeadEnds(roadLength);
        mapGrid.BuildGrid(roadHelper.GetRoadPositions(), (int)roadLength, (int)scaleFactor);
        transform.localScale = transform.localScale * scaleFactor;
    }

    public List<Road> GetRoads()
    {
        return roadHelper.GetRoads();
    }

    public List<Vector3Int> GetPositionsToSpawnBuildings()
    {
        return mapGrid.GetEmptyPositions();
    }

}
