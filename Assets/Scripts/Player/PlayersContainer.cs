using System.Collections.Generic;
using UnityEngine;

public class PlayersContainer : MonoBehaviour
{
    [SerializeField] List<PlayerController> _players = new();

    public List<PlayerController> Players { get => _players; set => _players = value; }

    private void Awake()
    {
        if (_players.Count == 0)
            Debug.Log($"No players inserted in {nameof(PlayersContainer)}");
    }
}
