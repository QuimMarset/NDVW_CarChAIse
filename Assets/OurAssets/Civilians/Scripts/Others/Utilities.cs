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
    
}