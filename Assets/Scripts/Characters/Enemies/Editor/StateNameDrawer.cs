using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateNameAttribute))]
public class StateNameDrawer : PropertyDrawer
{
    private List<string> stateNamesCache = new List<string>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use with string.");
            return;
        }

        StateMachineConfig config = property.serializedObject.targetObject as StateMachineConfig;
        if (config == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        stateNamesCache.Clear();
        foreach (StateMachineConfig.StateEntry state in config.states)
            if (!string.IsNullOrEmpty(state.stateName))
                stateNamesCache.Add(state.stateName);

        if (stateNamesCache.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        int currentIndex = Mathf.Max(0, stateNamesCache.IndexOf(property.stringValue));
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex, stateNamesCache.ToArray());

        if (newIndex >= 0 && newIndex < stateNamesCache.Count)
            property.stringValue = stateNamesCache[newIndex];
        else if (!string.IsNullOrEmpty(property.stringValue) && !stateNamesCache.Contains(property.stringValue))
            property.stringValue = "";
    }
}

