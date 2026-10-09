using System.Collections.Generic;
using UnityEngine;

public struct CameraBounds
{
    public float left;
    public float right;
    public float top;
    public float bottom;
    public float margin;
}

public class CoopCameraModel
{
    public Vector3 currentCentroid;
    public float BoundsMargin { get; set; }

    public CoopCameraModel(float boundsMargin)
    {
        BoundsMargin = boundsMargin;
    }

    public Vector3 FindCentroid(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 pos in positions)
            sum += pos;

        currentCentroid = sum / positions.Count;

        float maxDistance = 0f;

        foreach (Vector3 pos in positions)
            maxDistance = Mathf.Max(
                maxDistance,
                Vector3.Distance(currentCentroid, pos)
            );

        float halfFov = Camera.main.fieldOfView * 0.5f;

        float requiredZ = maxDistance / Mathf.Tan(halfFov * Mathf.Deg2Rad);

        requiredZ *= 1.1f;

        currentCentroid.z = -requiredZ;

        return currentCentroid;
    }


    public CameraBounds GetBounds(Vector3 camPos, float orthographicSize, float aspect)
    {
        float height = orthographicSize * 2f;
        float width = height * aspect;

        return new CameraBounds
        {
            left = camPos.x - width * 0.5f,
            right = camPos.x + width * 0.5f,
            bottom = camPos.y - height * 0.5f,
            top = camPos.y + height * 0.5f,
            margin = BoundsMargin
        };
    }
}