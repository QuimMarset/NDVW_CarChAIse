using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarCollisionBehavior))]
[CanEditMultipleObjects]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        CarCollisionBehavior carCollisionBehavior = (CarCollisionBehavior)target;
        Transform carTransform = carCollisionBehavior.transform;
        float fovRadius = carCollisionBehavior.FovRadius;
        float fovRadiusBackward = carCollisionBehavior.FovRadiusBackward;
        float fovAngle = carCollisionBehavior.FovAngle;
        float fovAngleBackward = carCollisionBehavior.FovAngleBackward;

        DrawFOV(carTransform, carTransform.forward, fovRadius, fovAngle, Color.red);
        DrawFOV(carTransform, -carTransform.forward, fovRadiusBackward, fovAngleBackward, Color.cyan);
    }

    private void DrawFOV(Transform carTransform, Vector3 direction, float fovRadius, float fovAngle, Color lineColor)
    {
        Handles.color = Color.white;
        Handles.DrawWireArc(carTransform.position, carTransform.up, direction, 360, fovRadius);

        Vector3 leftHalf = Quaternion.AngleAxis(-fovAngle / 2, carTransform.up) * direction;
        Vector3 rightHalf = Quaternion.AngleAxis(fovAngle / 2, carTransform.up) * direction;

        Handles.color = lineColor;
        Handles.DrawLine(carTransform.position, carTransform.position + leftHalf * fovRadius);
        Handles.DrawLine(carTransform.position, carTransform.position + rightHalf * fovRadius);
    }

}
