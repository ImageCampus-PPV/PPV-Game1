using Arch_IA_Piccioni.Voronoi;
using ImageCampus.ToolBox.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using EntityRegistry = GreenAbyss.Entities.EntityRegistry;
using ServiceProvider = GreenAbyss.Services.ServiceProvider;
using EventBus = GreenAbyss.Events.EventBus;
using IService = GreenAbyss.Services.IService;

public class NearestObjectDetector : ITickable, IService, IInitiable
{
    public bool IsPersistance => false;

    private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

    Voronoi<uint> dragonInteractableVoronoi;
    Voronoi<uint> mechaInteractableVoronoi;

    private Dictionary<uint, (uint objectID, Type objectType)> _interactablePerCharacter = new Dictionary<uint, (uint, Type)>();
    private float _tolerance = 3f;

    public Dictionary<uint, (uint objectID, Type objectType)> InteractablePerCharacter => _interactablePerCharacter;
    public (uint objectID, Type objectType) this[uint characterID] => _interactablePerCharacter[characterID];

    public void Tick(float deltaTime)
    {
        Interactable nearestInteractable = null;

        IEnumerator<Deposit> deposits = EntityRegistry.FilterEntities<Deposit>().GetEnumerator();

        deposits.MoveNext();

        Deposit deposit = deposits.Current;

        foreach (Character character in EntityRegistry.FilterEntities<Character>())
        {
            if (Vector3.Distance(character.transform.position, deposit.transform.position) < _tolerance)
            {
                _interactablePerCharacter[character.ID] = (deposit.ID, deposit.GetType());
                continue;
            }

            if (character.GetType() == typeof(Dragon))
            {
                if (dragonInteractableVoronoi.TryGetNearestObject(character.transform.position, out uint interactableID))
                {
                    nearestInteractable = EntityRegistry.GetAs<Interactable>(interactableID);

                    _interactablePerCharacter[character.ID] = Vector3.Distance(character.transform.position, nearestInteractable.transform.position) < _tolerance ? (nearestInteractable.ID, nearestInteractable.GetType()) : (BaseEntity.NULL_BASE_ENTITY, null);
                }
            }
            else
            {
                if (mechaInteractableVoronoi.TryGetNearestObject(character.transform.position, out uint interactableID))
                {
                    nearestInteractable = EntityRegistry.GetAs<Interactable>(interactableID);

                    _interactablePerCharacter[character.ID] = Vector3.Distance(character.transform.position, nearestInteractable.transform.position) < _tolerance ? (nearestInteractable.ID, nearestInteractable.GetType()) : (BaseEntity.NULL_BASE_ENTITY, null);
                }
            }

            _interactablePerCharacter[character.ID] = Vector3.Distance(character.transform.position, nearestInteractable.transform.position) < _tolerance ? (nearestInteractable.ID, nearestInteractable.GetType()) : (BaseEntity.NULL_BASE_ENTITY, null);
        }
    }

    public void Init()
    {
        dragonInteractableVoronoi = new Voronoi<uint>();
        mechaInteractableVoronoi = new Voronoi<uint>();
    }

    public void LateInit()
    {
        foreach (Interactable interactable in EntityRegistry.FilterEntities<DragonInteractable>())
            dragonInteractableVoronoi.Add(interactable.ID, interactable.transform.position);

        foreach (Interactable interactable in EntityRegistry.FilterEntities<MechaInteractable>())
            mechaInteractableVoronoi.Add(interactable.ID, interactable.transform.position);
    }
}
