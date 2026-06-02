using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InGameInspector : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] private Button _openMenuButton;
    [SerializeField] private Button _switchPlayerButton;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private Transform _contentContainer;
    [SerializeField] private DebugField _debugFieldPrefab;
    [SerializeField] private TextMeshProUGUI _targetPlayerText;
    [SerializeField] private GameObject _debugHeaderPrefab;

    [Header("Players")]
    [SerializeField] private PlayersContainer _playersContainer;

    private int _currentPlayerIndex = 0;

    private void Start()
    {
        _openMenuButton.onClick.AddListener(OpenMenu);
        _switchPlayerButton.onClick.AddListener(SwitchPlayer);
    }

    private void SwitchPlayer()
    {
        OnCyclePlayers();
    }

    private void OpenMenu()
    {
        _menuPanel.SetActive(!_menuPanel.activeSelf);
        RefreshUI();
    }

    public void OnCyclePlayers()
    {
        if (_menuPanel.activeSelf)
        {
            if (_playersContainer.Players.Count > 0)
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _playersContainer.Players.Count;
                RefreshUI();
            }
        }
    }

    private void RefreshUI()
    {
        foreach (Transform fieldPanel in _contentContainer)
        {
            Destroy(fieldPanel.gameObject);
        }

        if (_playersContainer == null || _playersContainer.Players.Count == 0)
        {
            _targetPlayerText.text = "No players online.";
        }

        if (_currentPlayerIndex >= _playersContainer.Players.Count)
        {
            _currentPlayerIndex = 0;
        }

        Character targetPlayer = _playersContainer.Players[_currentPlayerIndex];

        _targetPlayerText.text = $"Player {_currentPlayerIndex + 1}.";

        if (targetPlayer.ActiveMovement != null)
            GenerateUIForObject(targetPlayer.ActiveMovement, "Movement");
        if (targetPlayer.ActiveJump != null)
            GenerateUIForObject(targetPlayer.ActiveJump, "Jump");

        foreach (var ability in targetPlayer.ActiveAbilities)
        {
            GenerateUIForObject(ability, ability.GetType().Name);
            if (ability is ICombat)
            {
                var strategies = (ability as ICombat).Strategies;

                foreach (var strategy in strategies)
                {
                    GenerateUIForObject(strategy, strategy.GetType().Name);
                }
            }
        }
    }

    private void GenerateUIForObject(object targetObj, string headerName)
    {
        TextMeshProUGUI header = Instantiate(_debugHeaderPrefab, _contentContainer).GetComponentInChildren<TextMeshProUGUI>();
        header.text = headerName;

        Type currentType = targetObj.GetType();

        while (currentType != null && currentType != typeof(ScriptableObject) && currentType != typeof(object))
        {
            FieldInfo[] fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                bool isNumber = field.FieldType == typeof(int) || field.FieldType == typeof(float);
                bool isEditable = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;

                if (isNumber && isEditable)
                {
                    DebugField debugField = Instantiate(_debugFieldPrefab, _contentContainer);
                    debugField.Initialize(targetObj, field);
                }
            }

            currentType = currentType.BaseType;
        }

    }
}
