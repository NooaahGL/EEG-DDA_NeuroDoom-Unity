// Assets/Scripts/Enemies/EnemyConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Awareness")]
    public float awarenessRadius = 8f;
    public float alertDuration   = 5f;

    [Header("Attack")]
    public int   damage   = 10;
    public float cadence  = 1f;
    public float range    = 1.5f;

    [Header("Loot")]
    [Range(0,1)] public float dropChance = 0.4f;   // % de soltar algo
    [Range(0,1)] public float ammoWeight  = 0.5f;  // % dentro del drop
    [Range(0,1)] public float armorWeight = 0.2f;
    [Range(0,1)] public float healthWeight= 0.3f;
    [Range(0,1)] public float smallProb   = 0.5f;  // versión mini
}
