using ImageCampus.ToolBox.Services;
using System;
using UnityEngine;

[Serializable]
public struct PrefabByName
{
    public string TypeName;
    public GameObject gameObject;
}

[CreateAssetMenu(fileName = nameof(PrefabsRegistry), menuName = nameof(ScriptableObject) + "/" + nameof(PrefabsRegistry), order = 1)]
public class PrefabsRegistry : ScriptableObject, IService
{
    public bool IsPersistance => true;

    [SerializeField] private PrefabByName[] prefabs;

    public GameObject FindPrefabByName(string name)
    {
        foreach (PrefabByName prefabByName in prefabs)
            if (prefabByName.TypeName.Equals(name))
                return prefabByName.gameObject;

        return null;
    }
}
