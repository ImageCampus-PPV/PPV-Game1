using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersContainer : MonoBehaviour, IService
{
    [SerializeField] List<Character> _players = new();

    public IReadOnlyList<Character> Players => _players;

    public event Action<Character> OnPlayerAdded;
    public event Action<Character> OnPlayerRemoved;

    public bool IsPersistance => false;

    private void Awake()
    {
        ServiceProvider.Instance.AddService<PlayersContainer>(this);

        if (_players.Count == 0)
            Debug.Log($"No players inserted in {nameof(PlayersContainer)}");
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.RemoveService<PlayersContainer>();
    }

    public void AddPlayer(Character player)
    {
        if (player == null)
            return;

        if (_players.Contains(player))
            return;

        _players.Add(player);
        OnPlayerAdded?.Invoke(player);
    }

    public void RemovePlayer(Character player)
    {
        if (_players.Remove(player))
            OnPlayerRemoved?.Invoke(player);
    }
}
