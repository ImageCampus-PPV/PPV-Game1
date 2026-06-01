using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerJoinHandler : MonoBehaviour
{
    [SerializeField] private PlayersContainer _container;

    private void Awake()
    {
        if (_container == null)
        {
            Debug.LogError($"{nameof(PlayersContainer)} no asignado {nameof(PlayerJoinHandler)}");
        }     
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Character controller = playerInput.GetComponent<Character>();

        if (controller == null)
        {
            Debug.LogError("PlayerController no asignado");
            return;
        }

        _container.Players.Add(controller);
        Debug.Log($"Jugador {playerInput.playerIndex + 1} se ha unido. Total de jugadores: {_container.Players.Count}");
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Character controller = playerInput.GetComponent<Character>();

        if (controller != null)
        {
            _container.Players.Remove(controller);
        }
    }
}
