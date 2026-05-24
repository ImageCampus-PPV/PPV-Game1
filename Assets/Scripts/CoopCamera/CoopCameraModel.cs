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
    private readonly CoopCameraSettings _settings;

    public Vector3 currentCentroid { get; private set; }
    public float BoundsMargin { get; set; }

    public CoopCameraModel(CoopCameraSettings settings, float boundsMargin)
    {
        _settings = settings;
        BoundsMargin = boundsMargin;
    }

    public Vector3 FindCentroid(List<Vector3> positions)
    {
        Vector3 sum = Vector3.zero;

        if (positions.Count == 0)
            return sum;

        foreach (var pos in positions)
            sum += pos;

        currentCentroid = sum / positions.Count;
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