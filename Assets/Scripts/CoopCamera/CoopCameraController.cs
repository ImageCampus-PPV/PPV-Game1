using System;
using UnityEngine;

[Serializable]
public class CoopCameraSettings
{
    public Vector3 offset;
    public float speed;
}

public interface ICoopCameraService : IService
{
    CameraBounds GetBounds();
}

public class CoopCameraController : MonoBehaviour, ICoopCameraService
{
    [SerializeField] private Camera _cam;
    [SerializeField] private PlayersContainer _container;
    [SerializeField] private CoopCameraSettings _settings = new();
    [SerializeField] private float _boundsMargin = 0.5f;

    private CoopCameraModel _model;

    public bool IsPersistent => false;

    private void Awake()
    {
        _model = new CoopCameraModel(_settings, _boundsMargin);

        ServiceProvider.Instance.AddService<ICoopCameraService>(this);

        if (_container == null)
            Debug.Log($"No {nameof(PlayersContainer)} inserted in {nameof(CoopCameraController)}");

        if (_cam == null)
            _cam = GetComponent<Camera>();

        SnapToPlayer();
    }
    private void LateUpdate()
    {
        if (_container == null || _container.Players.Count == 0)
            return;

        var positions = _container.Players.ConvertAll(player => player.transform.position);

        Vector3 targetCentroid = _model.FindCentroid(positions);

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

    private void SnapToPlayer()
    {
        if (_container.Players.Count == 0)
            return;

        var startPos = _container.Players[0].transform.position;

        transform.position = startPos += _settings.offset;
    }

    private void GoToPos(Vector3 pos)
    {
        Vector3 desiredPos = pos + _settings.offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, _settings.speed * Time.deltaTime);
        transform.position = smoothedPos;
    }
}
