using TMPro;
using UnityEngine;

public class StateDebugDisplay : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _stateOwner; 
    [SerializeField] private TextMeshProUGUI _text;

    private IStateDebugInfo _debug;

    private void Awake()
    {
        _debug = _stateOwner as IStateDebugInfo;
        if (_debug == null)
            Debug.LogError("State owner does not implement IStateDebugInfo");
    }

    private void Update()
    {
        if (_debug != null && _text != null)
            _text.text = _debug.CurrentStateName;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stateOwner == null)
            return;

        if (_stateOwner is not IStateDebugInfo)
        {
            Debug.LogError("State owner does not implement IStateDebugInfo interface.");
            _stateOwner = null;
        }
    }
#endif
}