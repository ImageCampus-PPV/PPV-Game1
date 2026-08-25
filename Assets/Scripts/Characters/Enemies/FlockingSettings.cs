using System;
using UnityEngine;

[Serializable]
public struct FlockingSettings
{
    [Header("General")]
    [SerializeField] public bool enabled;

    [Header("Flock separation")]
    [SerializeField] public float separationRadius;
    [SerializeField] public float separationWeight;
    [SerializeField] public float separationPredictionTime;

    [Header("Obstacle avoidance")]
    [SerializeField] public float obstacleLookDistance;
    [SerializeField] public float obstacleWeight;
    [SerializeField] public float obstacleNormalWeight;
    [SerializeField] public float obstacleTangentWeight;

    [Header("Seek")]
    [SerializeField] public float seekWeight;

    [Header("Body")]
    [SerializeField] public float bodyRadius;

    [Header("Wander")]
    [SerializeField] public float wanderWeight;
    [SerializeField] public float wanderJitter;
    [SerializeField] public float wanderStrength;

    [Header("Orbit")]
    [SerializeField] public float orbitWeight;
    [SerializeField] public float orbitMinDistance;
    [SerializeField] public float orbitMaxDistance;
}
