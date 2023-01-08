using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CityGenerator : MonoBehaviour
{

    public static CityGenerator instance;
    public List<GameObject> generatedObjects = new List<GameObject>();

    public PerlinGenerator perlinGenerator;
    public BuildingSpawner buildingSpawner;

    public bool debugMode;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Generate();
    }

    void Update()
    {
        if (debugMode && Input.GetButtonDown("Jump"))
        {
            ClearAllObjects();
            Generate();
        }
    }


    public void AddObject(GameObject objectToAdd)
    {
        generatedObjects.Add(objectToAdd);
    }

    public void Generate()
    {
        perlinGenerator.Generate();
        buildingSpawner.Generate();
    }


    void ClearAllObjects()
    {
        for (int i = generatedObjects.Count - 1; i >= 0; i--)
        {
            generatedObjects[i].SetActive(false);
            Destroy(generatedObjects[i]);
            generatedObjects.RemoveAt(i);
        }
    }
}
