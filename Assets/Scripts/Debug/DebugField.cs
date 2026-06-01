using System;
using System.Reflection;
using TMPro;
using UnityEngine;

public class DebugField : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fieldNameText;
    [SerializeField] private TMP_InputField _InputField;

    private object _targetObject;
    private FieldInfo _fieldInfo;

    public void Initialize(object target, FieldInfo field)
    {
        _targetObject = target;
        _fieldInfo = field;

        _fieldNameText.text = field.Name.Replace("_", "");
        _InputField.text = field.GetValue(target).ToString();

        _InputField.onEndEdit.AddListener(OnValueChange);
    }

    private void OnValueChange(string newValue)
    {
        try
        {
            if (_fieldInfo.FieldType == typeof(float))
            {
                _fieldInfo.SetValue(_targetObject, float.Parse(newValue));
            }
            else if (_fieldInfo.FieldType == typeof(int))
            {
                _fieldInfo.SetValue(_targetObject, int.Parse(newValue));
            }
        }
        catch (Exception except)
        {
            Debug.LogWarning($"Invalid value: {except.Message}");
            _InputField.text = _fieldInfo.GetValue(_targetObject).ToString();
            throw;
        }
    }
}
