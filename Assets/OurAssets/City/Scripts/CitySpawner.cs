using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySpawner : MonoBehaviour
{

    public int gridX = 7;
    public int gridZ = 7;
    public float gridOffset = 30f;
    public float minDistancePlayer = 30f;
    public Vector3 gridOrigin = Vector3.zero;
    public GameObject buildingPrefab;
    public GameObject roadPrefab;
    public GameObject intersectionPrefab;
    public GameObject player;
    public GameObject police;

    public void Generate()
    {
        SpawnCity();
    }

    void SpawnCity()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                GameObject obj;
                Vector3 position = transform.position + gridOrigin;

                if ((x % 2 == 0) && (z % 2 == 0))
                {
                    position += new Vector3(gridOffset * x, 0.5f, gridOffset * z);
                    obj = Instantiate(buildingPrefab, position, transform.rotation);
                }
                else
                {
                    position += new Vector3(gridOffset * x, -3.0f, gridOffset * z);

                    if ((x + z) % 2 == 0)
                    {
                        obj = Instantiate(intersectionPrefab, position, transform.rotation);
                    }
                    else
                    {
                        if (x % 2 == 0)
                        {
                            obj = Instantiate(roadPrefab, position, transform.rotation);
                        }
                        else
                        {
                            obj = Instantiate(roadPrefab, position, transform.rotation);
                            Vector3 center = obj.GetComponentInChildren<Renderer>().bounds.center;
                            obj.transform.RotateAround(center, Vector3.up, 90.0f);

                            float distance = Vector3.Distance(position, player.transform.position);
                            if (distance >= minDistancePlayer)
                            {
                                GameObject policeCar = Instantiate(police, center + new Vector3(0f, 10f), transform.rotation);
                                GeneratedObjectControl.instance.AddObject(policeCar);
                            }
                        }
                    }
                    GeneratedObjectControl.instance.AddObject(obj);
                }

                obj.transform.SetParent(this.transform);
            }
        }
    }


}
