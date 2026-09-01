using UnityEngine;
using Assets.Scripts.Entities;
using System.Collections.Generic;
using ImageCampus.ToolBox.Services;
using System;

public class CoopCameraController : IInitiable, ITickable, IService
{
    public bool IsPersistance => false;

    EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private float speed = 10.0f;
    private Vector3 offset = Vector3.forward * -10.0f;

    [SerializeField] private Camera _cam;

    [SerializeField] private float _boundsMargin = 0.5f;

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
        _cam.transform.position = smoothedPos;
    }

    public void SetOffset(Vector3 offset)
    {
        this.offset = offset;
    }

    public void SetOffset(float offsetZ)
    {
        offset = Vector3.forward * -Mathf.Abs(offsetZ);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public bool IsInCameraBounds(Vector3 position)
    {
        Vector3 viewportPos = _cam.WorldToViewportPoint(position);

        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f &&
               viewportPos.z > 0f;
    }
}
