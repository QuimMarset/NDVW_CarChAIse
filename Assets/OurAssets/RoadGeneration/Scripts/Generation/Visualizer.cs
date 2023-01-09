using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{
    public LSystemGenerator lSystem;
    
    List<Vector3> positions = new();
    public GameObject prefab;
    public Material lineMaterial;

    public float roadlength;
    public int numInBetween;

    public float angle = 90;

    private void Start()
    {
        VisualizeSequence();
    }

    private void VisualizeSequence()
    {
        string finalSentence = lSystem.finalSentence;


        Stack<AgentParameters> savedPoints = new();
        Vector3 currentPosition = Vector3.zero;
        Vector3 direction = Vector3.forward;
        Vector3 tempPosition = Vector3.zero;

        positions.Add(currentPosition);

        foreach (char letter in finalSentence)
        {
            EncodingLetters encodedLetter = (EncodingLetters)letter;

            switch (encodedLetter)
            {
                case EncodingLetters.save:

                    savedPoints.Push(new AgentParameters
                    {
                        position = currentPosition,
                        direction = direction,
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
                    currentPosition += direction * numInBetween * roadlength;
                    DrawLine(tempPosition, currentPosition);
                    positions.Add(currentPosition);

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

        foreach (Vector3 position in positions)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }

    }

    private void DrawLine(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject line = new GameObject("line");
        line.transform.position = startPosition;
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }

    public enum EncodingLetters
    {
        unknown = '1',
        save = '[',
        load = ']',
        draw = 'F',
        turnRight = '+',
        turnLeft = '-'
    }
}
