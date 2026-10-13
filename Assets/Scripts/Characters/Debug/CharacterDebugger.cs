using TMPro;
using UnityEngine;

public class CharacterDebugger : MonoBehaviour
{
    [SerializeField] private Character _character;
    [SerializeField] private TextMeshProUGUI _characterNameText;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private CharacterDebugInfo _debugInfo;
    public CharacterDebugInfo DebugInfo { get => _debugInfo; set => _debugInfo = value; }


    public void UpdateInfo()
    {
        if (_debugInfo == null)
            return;

        if (_characterNameText != null)
            _characterNameText.text = _debugInfo.CharacterName;

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _spriteRenderer.color = _debugInfo.DebugColor;
        //_collector.Filter = _debugInfo.ItemFilter;

        if (_character != null)
            _character.EquipCharacter(_debugInfo);
    }
}
