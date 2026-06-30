using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateNameAttribute))]
public class StateNameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use with string.");
            return;
        }

        var config = property.serializedObject.targetObject as StateMachineConfig;
        if (config == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var stateNames = new List<string>();
        foreach (var state in config.states)
            if (!string.IsNullOrEmpty(state.stateName))
                stateNames.Add(state.stateName);

        if (stateNames.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.LabelField(new Rect(position.x + position.width - 150, position.y, 150, position.height),
                " (add states first)", EditorStyles.miniLabel);
            return;
        }

        int currentIndex = Mathf.Max(0, stateNames.IndexOf(property.stringValue));
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, stateNames.ToArray());
        if (newIndex >= 0 && newIndex < stateNames.Count)
            property.stringValue = stateNames[newIndex];
        else if (!string.IsNullOrEmpty(property.stringValue) && !stateNames.Contains(property.stringValue))
            property.stringValue = "";
    }
}

