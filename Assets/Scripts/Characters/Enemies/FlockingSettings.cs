using System;
using UnityEngine;

[Serializable]
public struct FlockingSettings
{
    [SerializeField] public float separationRadius;
    [SerializeField] public float obstacleLookRadius;

    [SerializeField] public float separationWeight;
    [SerializeField] public float obstacleWeight;
    [SerializeField] public float seekWeight;

    [SerializeField] public float bodyRadius;

    [SerializeField] public float wanderWeight;
    [SerializeField] public float wanderJitter;
    [SerializeField] public float wanderRadius;

    [SerializeField] public float orbitWeight;
    [SerializeField] public float orbitMinDistance;
    [SerializeField] public float orbitMaxDistance;
}
