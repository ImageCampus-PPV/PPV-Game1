using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[Serializable]
public struct PrefabByName
{
    public string TypeName; 
    public GameObject gameObject;
}

[CreateAssetMenu(fileName = nameof(PrefabsRegistry), menuName =  nameof(ScriptableObject) +  "/" + nameof(PrefabsRegistry), order = 1)]
public class PrefabsRegistry : ScriptableObject, IService
{
    public bool IsPersistance => true;

    public PrefabByName[] prefab;
}
