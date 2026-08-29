using System;
using UnityEngine;
using ImageCampus.ToolBox.Services;
using System.Collections.Generic;

[Serializable]
public class CoopCameraSettings
{
    public Vector3 offset;
    public float speed;
}

public class CoopCameraController : MonoBehaviour, IService
{
    [SerializeField] private Camera _cam;
    [SerializeField] private PlayersContainer _container;
    [SerializeField] private CoopCameraSettings _settings = new();
    [SerializeField] private float _boundsMargin = 0.5f;

    private CoopCameraModel _model;
    private readonly List<Vector3> _positions = new();

    public bool IsPersistance => false;

    private void Awake()
    {
        _model = new CoopCameraModel(_settings, _boundsMargin);

        ServiceProvider.Instance.AddService<CoopCameraController>(this);

        if (_container == null)
            Debug.Log($"No {nameof(PlayersContainer)} inserted in {nameof(CoopCameraController)}");

        if (_cam == null)
            _cam = GetComponent<Camera>();

        SnapToPlayer();
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.RemoveService<CoopCameraController>();
    }

    private void LateUpdate()
    {
        if (_container == null || _container.Players.Count == 0)
            return;

        _positions.Clear();

        foreach (Character player in _container.Players)
            _positions.Add(player.transform.position);

        Vector3 targetCentroid = _model.FindCentroid(_positions);

        GoToPos(targetCentroid);
    }

    public CameraBounds GetBounds()
    {
        return _model.GetBounds(_cam.transform.position, _cam.orthographicSize, _cam.aspect);
    }

    private void SnapToPlayer()
    {
        if (_container.Players.Count == 0)
            return;

        Vector3 startPos = _container.Players[0].transform.position;

        transform.position = startPos += _settings.offset;
    }

    private void GoToPos(Vector3 pos)
    {
        Vector3 desiredPos = pos + _settings.offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, _settings.speed * Time.deltaTime);
        transform.position = smoothedPos;
    }

    public bool IsInCameraBounds(Vector3 position)
    {
        Vector3 viewportPos = _cam.WorldToViewportPoint(position);

        return viewportPos.x >= 0f && viewportPos.x <= 1f && 
               viewportPos.y >= 0f && viewportPos.y <= 1f && 
               viewportPos.z > 0f;
    }
}
