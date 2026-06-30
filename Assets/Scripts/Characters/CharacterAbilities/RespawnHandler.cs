using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    [SerializeField] private Transform[] _respawnPoint;

    private PlayersContainer _container;

    private void Start()
    {
        _container = ServiceProvider.Instance.GetService<PlayersContainer>();

        foreach (Character player in _container.Players)
            AddCheckTeamWipe(player);

        _container.OnPlayerAdded += OnPlayerJoin;
        _container.OnPlayerRemoved += OnPlayerLeft;
    }

    private void OnPlayerLeft(Character character)
    {
        RemoveCheckTeamWipe(character);
    }

    private void RemoveCheckTeamWipe(Character character)
    {
        if (character.TryGetComponent(out Health health))
            health.OnDowned -= CheckTeamWipe;
    }

    private void OnPlayerJoin(Character character)
    {
        AddCheckTeamWipe(character);
    }

    void AddCheckTeamWipe(Character character)
    {
        if (character.TryGetComponent(out Health health))
            health.OnDowned += CheckTeamWipe;
    }

    private void OnDestroy()
    {
        if (_container == null) return;

        foreach (Character player in _container.Players)
            RemoveCheckTeamWipe(player);
    }

    private void CheckTeamWipe(MonoBehaviour damageable)
    {
        foreach (Character player in _container.Players)
            if (player.TryGetComponent(out Health health))
                if (!health.IsDowned)
                    return;

        Debug.Log("Respawning...");
        RespawnAll();
    }

    private void RespawnAll()
    {
        if (_respawnPoint == null || _respawnPoint.Length == 0)
        {
            Debug.LogWarning("No respawn points assigned");
            return;
        }

        for (int i = 0; i < _container.Players.Count; i++)
        {
            Character player = _container.Players[i];
            Transform spawnPoint = _respawnPoint[i % _respawnPoint.Length];
            player.transform.position = spawnPoint.position;

            if (player.TryGetComponent(out Health health))
                health.Reset();
        }
    }
}
