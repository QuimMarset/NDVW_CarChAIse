using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySpawner : MonoBehaviour
{

    public int gridX = 7;
    public int gridZ = 7;
    public GameObject buildingPrefab;
    public GameObject roadPrefab;
    public GameObject intersectionPrefab;

    public Vector3 gridOrigin = Vector3.zero;
    public float gridOffset = 30f;

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
                        }
                    }
                    GeneratedObjectControl.instance.AddObject(obj);
                }

                obj.transform.SetParent(this.transform);
            }
        }
    }


}
