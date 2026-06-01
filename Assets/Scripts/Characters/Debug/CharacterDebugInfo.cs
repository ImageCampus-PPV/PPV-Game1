using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDebugInfo", menuName = "Debug/CharacterDebugInfo")]
public class CharacterDebugInfo : ScriptableObject
{
    [Header("Visual")]
    [SerializeField] private Color _debugColor;
    [SerializeField] private string _characterName;
    [SerializeField] private ItemFilter _filter;

    [Header("Abilities")]
    [SerializeField] private MovementAbility _movementAbility;
    [SerializeField] private JumpAbility _jumpAbility;
    [SerializeField] private CharacterAbility[] _abilities;

    public string CharacterName => _characterName;
    public Color DebugColor => _debugColor;
    public ItemFilter ItemFilter => _filter;
    public MovementAbility MovementAbility => _movementAbility;
    public JumpAbility JumpAbility => _jumpAbility;
    public CharacterAbility[] Abilities => _abilities;
}
