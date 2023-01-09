using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingGeneratorNoiseInput : MonoBehaviour
{
    public int maxPieces = 20;
    public float scaleFactor = 2f;

    public int randomVariationMin = -5;
    public int randomVariationMax = 10;
    public GameObject[] baseParts;
    public GameObject[] middleParts;
    public GameObject[] topParts;

    void Start()
    {
        Build();
    }


    public void Build()
    {
        float sampledValue = PerlinGenerator.instance.PerlinSteppedPosition(transform.position);

        int targetPieces = Mathf.FloorToInt(maxPieces * (sampledValue));
        targetPieces += Random.Range(randomVariationMin, randomVariationMax);

        if (targetPieces <= 0)
        {
            return;
        }

        float heightOffset = 0;
        heightOffset += SpawnPieceLayer(baseParts, heightOffset);

        for (int i = 2; i < targetPieces; i++)
        {
            heightOffset += SpawnPieceLayer(middleParts, heightOffset);
        }

        SpawnPieceLayer(topParts, heightOffset);
    }

    float SpawnPieceLayer(GameObject[] pieceArray, float inputHeight)
    {
        Transform randomTransform = pieceArray[Random.Range(0, pieceArray.Length)].transform;

        GameObject piece = Instantiate(randomTransform.gameObject, this.transform.position + new Vector3(0, inputHeight, 0), transform.rotation) as GameObject;

        piece.transform.localScale *= scaleFactor;

        Mesh cloneMesh = piece.GetComponentInChildren<MeshFilter>().mesh;
        Bounds baseBounds = cloneMesh.bounds;
        float heightOffset = baseBounds.size.y * scaleFactor;

        piece.transform.SetParent(this.transform);

        // GeneratedObjectControl.instance.AddObject(piece);
        CityGenerator.instance.AddObject(piece);

        return heightOffset;
    }

}
