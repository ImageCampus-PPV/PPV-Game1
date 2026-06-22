using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    [SerializeField] private Transform[] _respawnPoint;

    private PlayersContainer container => ServiceProvider.Instance.GetService<PlayersContainer>();

    private void Start()
    {
        foreach (Character player in container.Players)
        {
            AddCheckTeamWipe(player);
        }

        container.OnPlayerAdded += OnPlayerJoin;
        container.OnPlayerRemoved += OnPlayerLeft;
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
        foreach (Character player in container.Players)
        {
            RemoveCheckTeamWipe(player);
        }
    }

    private void CheckTeamWipe()
    {
        foreach (Character player in container.Players)
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

        for (int i = 0; i < container.Players.Count; i++)
        {
            Character player = container.Players[i];

            Transform spawnPoint = _respawnPoint[i % _respawnPoint.Length];
            player.transform.position = spawnPoint.position;

            if (player.TryGetComponent(out Health health))
                health.Reset();
        }
    }
}