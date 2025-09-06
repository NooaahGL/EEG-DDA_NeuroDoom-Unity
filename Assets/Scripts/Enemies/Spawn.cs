using UnityEngine;

public class Spawn : MonoBehaviour

{
    public enum SpawnType { Enemy, Health, Ammo }
    public SpawnType type = SpawnType.Enemy;
}
