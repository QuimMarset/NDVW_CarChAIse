using UnityEngine;


public static class Utilities
{
    
    public static bool DestinationReached(Vector3 currentPosition, Vector3 targetPosition, float threshold)
    {
        Vector2 currentPositionNoUp = new(currentPosition.x, currentPosition.z);
        Vector2 targetPositionNoUp = new(targetPosition.x, targetPosition.z);
        float distance = Vector2.Distance(currentPositionNoUp, targetPositionNoUp);
        return (distance < threshold);
    }

    public static Road FindRoadUnderCar(Transform carTransform)
    {
        int layerMask = LayerMask.GetMask(new string[] { "Road" });

        Collider[] colliders = Physics.OverlapSphere(carTransform.position, 3, layerMask);
        if (colliders.Length > 0)
        {
            Road road = colliders[0].GetComponent<Road>();
            return road;
        }

        colliders = Physics.OverlapCapsule(carTransform.position + Vector3.up * 5, 
            carTransform.position - Vector3.up * 5, 3, layerMask);

        if (colliders.Length > 0)
        {
            Road road = colliders[0].GetComponent<Road>();
            return road;
        }

        Debug.Log(carTransform.gameObject.name + " " + "UNDEFINED ROAD");
        return null;
    }

}