using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingSpawner : MonoBehaviour
{
	public GameObject buildingPrefab;

	public RoadsGenerator roadsGenerator;

	public void Generate()
	{
		SpawnBuildings();
	}

	void SpawnBuildings()
	{
		List<Vector3Int> spawnPoints = roadsGenerator.GetPositionsToSpawnBuildings();
		Debug.Log(spawnPoints.Count);

		BoxCollider collider;
		NavMeshObstacle obstacle;
		float[] rotations = { 0, 90, 180, 270 };
		float buildingsScale = 2.75f * roadsGenerator.scaleFactor;
		foreach (Vector3Int position in spawnPoints)
		{
			GameObject obj = Instantiate(buildingPrefab, position, transform.rotation);
			obj.transform.SetParent(this.transform);

			// Add BoxCollider and set
			collider = obj.AddComponent<BoxCollider>();
			collider.size = Vector3.one * buildingsScale;
			collider.center += Vector3.up * buildingsScale / 2;

			// Add NavMeshObstacle and set
			obstacle = obj.AddComponent<NavMeshObstacle>();
			obstacle.size = Vector3.one * buildingsScale;
			obstacle.center += Vector3.up * buildingsScale / 2;
			obstacle.carving = true;

			// Random rotation
			obj.transform.Rotate(Vector3.up, rotations[Random.Range(0, rotations.Length)]);

			CityGenerator.instance.AddObject(obj);
		}
	}
}
