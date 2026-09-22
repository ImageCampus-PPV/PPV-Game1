using ImageCampus.ToolBox.Services;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvas : MonoBehaviour, IService, IInitiable
{
    public bool IsPersistance => false;

    private Dictionary<string, GameObject> _staticUIObjects;
    private Dictionary<string, GameObject> _dynamicUIObjects;

    public void Init()
    {
        _staticUIObjects = new Dictionary<string, GameObject>();
        _dynamicUIObjects = new Dictionary<string, GameObject>();

        foreach (Transform child in transform)
            _staticUIObjects.Add(child.name, child.gameObject);
    }

    public void LateInit()
    {

    }

    public void AddDynmic(string name, GameObject newGameObject)
    {
        _dynamicUIObjects.Add(name, newGameObject);

        newGameObject.transform.SetParent(transform, false);
    }

    public GameObject Get(string name)
    {
        if (_dynamicUIObjects.ContainsKey(name))
            return _dynamicUIObjects[name];

        if (_staticUIObjects.ContainsKey(name))
            return _staticUIObjects[name];

        return null;
    }

    public void RemoveDynmic(string name)
    {
        _dynamicUIObjects.Remove(name);
    }
}
