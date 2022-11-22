using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CivilianAI))]
[CanEditMultipleObjects]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        CivilianAI civilianAI = (CivilianAI)target;
        Transform carFront = civilianAI.carFront;
        DrawCollisionFOV(carFront, civilianAI);
        // DrawObstacleFOV(carFront, civilianAI);
    }

    private void DrawCollisionFOV(Transform carFront, CivilianAI civilianAI)
    {
        DrawFOV(carFront, civilianAI.fovRadius, civilianAI.fovAngle);
    }

    private void DrawObstacleFOV(Transform carFront, CivilianAI civilianAI)
    {
        // DrawFOV(carFront, civilianAI.fovObstacleRadius, civilianAI.fovObstacleAngle);
    }

    private void DrawFOV(Transform carFront, float fovRadius, float fovAngle)
    {
        Handles.color = Color.white;
        Handles.DrawWireArc(carFront.position, Vector3.up, Vector3.forward, 360, fovRadius);

        Vector3 viewAngle01 = DirectionFromAngle(carFront.transform.eulerAngles.y, -fovAngle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(carFront.transform.eulerAngles.y, fovAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(carFront.position, carFront.position + viewAngle01 * fovRadius);
        Handles.DrawLine(carFront.position, carFront.position + viewAngle02 * fovRadius);
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
