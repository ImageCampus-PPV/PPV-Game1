using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeDebugVisual : MonoBehaviour, IService
{
    private struct TimedLine
    {
        public LineRenderer line;
        public float timeRemaining;
    }

    [SerializeField] private Material _debugMat;
    [SerializeField] private int _circleSegments = 24;
    private List<LineRenderer> _linePool = new();
    private List<TimedLine> _activeTimedLines = new();

    public bool IsPersistance => false;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<RuntimeDebugVisual>(this);
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.RemoveService<RuntimeDebugVisual>();
    }

    private void Update()
    {
        for (int i = _activeTimedLines.Count -1; i >= 0; i--)
        {
            TimedLine timedLine = _activeTimedLines[i];
            timedLine.timeRemaining -= Time.deltaTime;

            if (timedLine.timeRemaining <= 0f)
            {
                ReturnLine(timedLine.line);
                _activeTimedLines.RemoveAt(i);
            }
            else
                _activeTimedLines[i] = timedLine;
        }
    }

    public LineRenderer RequestPersistentLine(Color color, float thickness)
    {
        LineRenderer line = GetOrCreateLine();
        line.startColor = color;
        line.endColor = color;
        line.startWidth = thickness;
        line.endWidth = thickness;
        line.gameObject.SetActive(true);
        return line;
    }

    private LineRenderer GetOrCreateLine()
    {
        foreach (var pooledLine in _linePool)
        {
            if (!pooledLine.gameObject.activeSelf)
                return pooledLine;
        }

        GameObject lineGO = new GameObject($"Debug_Line");
        lineGO.transform.SetParent(transform);
        LineRenderer line = lineGO.AddComponent<LineRenderer>();
        line.material = _debugMat;
        line.useWorldSpace = true;
        line.gameObject.SetActive(false);
        _linePool.Add(line);
        return line;
    }

    private void ReturnLine(LineRenderer line)
    {
        if (line == null)
            return;

        line.positionCount = 0;
        line.gameObject.SetActive(false);
    }

    public void DrawRay(Vector2 start, Vector2 dir, float length,
                        Color color, float duration, float thickness = 0.05f)
    {
        LineRenderer line = RequestPersistentLine(color, thickness);
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, start + (dir * length));

        _activeTimedLines.Add(new TimedLine
        {
            line = line,
            timeRemaining = duration,
        });
    }

    public void DrawCircle(Vector2 center, float radius, Color color,
                           float duration, float thickness = 0.05f)
    {
        LineRenderer line = RequestPersistentLine(color, thickness);

        line.positionCount = _circleSegments + 1;

        for (int i = 0; i <= _circleSegments; i++)
        {
            float alpha = (i / (float)_circleSegments) * Mathf.PI * 2f;
            float x = Mathf.Cos(alpha) * radius;
            float y = Mathf.Sin(alpha) * radius;
            line.SetPosition(i, center + new Vector2(x, y));
        }

        _activeTimedLines.Add(new TimedLine
        {
            line = line,
            timeRemaining = duration,
        });
    }

    public void DrawBox(Vector2 center, Vector2 size, Color color,
                           float duration, float thickness = 0.05f)
    {
        LineRenderer line = RequestPersistentLine(color, thickness);
        line.positionCount = 5;

        Vector2 extents = size / 2f;

        line.SetPosition(0, center + new Vector2(-extents.x, extents.y));
        line.SetPosition(1, center + new Vector2(extents.x, extents.y));
        line.SetPosition(2, center + new Vector2(extents.x, -extents.y));
        line.SetPosition(3, center + new Vector2(-extents.x, -extents.y));
        line.SetPosition(4, center + new Vector2(-extents.x, extents.y));

        _activeTimedLines.Add(new TimedLine
        {
            line = line,
            timeRemaining = duration,
        });
    }

    internal void DrawOrientedBox(Vector2 center, Vector2 size, float angle, Color color, float duration, float thickness)
    {
        LineRenderer line = RequestPersistentLine(color, thickness);
        line.positionCount = 5;

        Vector2 half = size * 0.5f;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        Vector2 p0 = center + (Vector2)(rot * new Vector2(-half.x, half.y));
        Vector2 p1 = center + (Vector2)(rot * new Vector2(half.x, half.y));
        Vector2 p2 = center + (Vector2)(rot * new Vector2(half.x, -half.y));
        Vector2 p3 = center + (Vector2)(rot * new Vector2(-half.x, -half.y));

        line.SetPosition(0, p0);
        line.SetPosition(1, p1);
        line.SetPosition(2, p2);
        line.SetPosition(3, p3);
        line.SetPosition(4, p0);

        _activeTimedLines.Add(new TimedLine
        {
            line = line,
            timeRemaining = duration,
        });
    }
}