using UnityEngine;
using System.Collections.Generic;
using GreenAbyss.Services;
using GreenAbyss.Entities;

public class CoopCameraController : IInitiable, ITickable, IService
{
    public bool IsPersistance => false;

    EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<GreenAbyss.Entities.EntityRegistry>();

    private float speed = 10.0f;
    private Vector3 offset = Vector3.forward * -10.0f;

    private Camera _cam;

    private float _boundsMargin = 0.5f;
    private float _maxZoomOut = -21.8f;
    private float _maxY = 4.86f;
    private float _minY = -46.92f;
    private float _minX = -0.38f;
    private float _maxX = 0.48f;


    private CoopCameraModel _model;
    private readonly List<Vector3> _positions = new();

    public Camera Camera => _cam;

    public void Init()
    {
        _model = new CoopCameraModel(_boundsMargin);

        _cam = Camera.main;
    }

    public void LateInit()
    {

    }

    public void Tick(float deltaTime)
    {
        _positions.Clear();

        foreach (Character player in EntityRegistry.FilterEntities<Character>())
            _positions.Add(player.transform.position);

        Vector3 targetCentroid = _model.FindCentroid(_positions);

        GoToPos(targetCentroid);
    }

    public CameraBounds GetBounds()
    {
        float height = _cam.orthographicSize * 2f;
        float width = height * _cam.aspect;

        Vector3 pos = _cam.transform.position;

        return new CameraBounds
        {
            left = pos.x - width * 0.5f,
            right = pos.x + width * 0.5f,
            bottom = pos.y - height * 0.5f,
            top = pos.y + height * 0.5f,
            margin = _boundsMargin
        };
    }

    private void GoToPos(Vector3 pos)
    {
        Vector3 desiredPos = pos + offset;
        Vector3 smoothedPos = Vector3.Lerp(_cam.transform.position, desiredPos, speed * Time.deltaTime);

        smoothedPos = new Vector3(Mathf.Clamp(smoothedPos.x, _minX, _maxX), Mathf.Clamp(smoothedPos.y, _minY, _maxY), Mathf.Clamp(smoothedPos.z, _maxZoomOut, offset.z));
        
        _cam.transform.position = smoothedPos;
    }

    public bool IsInCameraBounds(Vector3 position)
    {
        Vector3 viewportPos = _cam.WorldToViewportPoint(position);

        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
    }
}
