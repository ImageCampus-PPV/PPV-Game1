using Assets.Scripts.Entities;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NearestObjectDetector : ITickable, IService
{
    public bool IsPersistance => false;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    private Dictionary<uint, (uint, Type)> _interactablePerCharacter = new Dictionary<uint, (uint, Type)>();
    private float _tolerance = 5f;

    public Dictionary<uint, (uint objectID, Type objectType)> InteractablePerCharacter => _interactablePerCharacter;
    public (uint objectID, Type objectType) this[uint characterID] => _interactablePerCharacter[characterID];

    public void Tick(float deltaTime)
    {
        foreach (Character character in EntityRegistry.FilterEntities<Character>())
        {
            float nearestDistance = float.MaxValue;
            (uint, Type) nearestObject = (BaseEntity.NULL_BASE_ENTITY, null);

            foreach (Interactable interactable in EntityRegistry.FilterEntities<Interactable>())
            {
                float distance = Vector3.SqrMagnitude(character.transform.position - interactable.transform.position);

                if (distance >= nearestDistance)
                    continue;

                nearestDistance = distance;
                nearestObject = (interactable.ID, interactable.GetType());
            }

            _interactablePerCharacter[character.ID] = nearestDistance < _tolerance * _tolerance ? nearestObject : (BaseEntity.NULL_BASE_ENTITY, null);
        }
    }

}
