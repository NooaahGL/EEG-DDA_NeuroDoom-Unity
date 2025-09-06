using UnityEngine;

public class SpawnPoint : MonoBehaviour   // ← debe heredar
{
    public enum SpawnType { Enemy, Health, Ammo }
    public SpawnType type = SpawnType.Enemy;
}
