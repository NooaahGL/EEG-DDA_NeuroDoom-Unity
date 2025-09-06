// Assets/Scripts/Enemies/EnemyManager.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    [Header("Configuración")]
    public EnemyConfig eConfig;

    [Header("Configuración global del enemigo")]

    public List<Enemy> enemiesInTrigger { get; private set; } = new();
    public int EnemiesCount => enemiesInTrigger.Count;

    public void AddEnemy(Enemy enemy)
    {
        if (enemy == null || enemiesInTrigger.Contains(enemy)) return;

        enemiesInTrigger.Add(enemy);
        ApplyConfig(enemy);                   
    }

    public void RemoveEnemy(Enemy enemy) => enemiesInTrigger.Remove(enemy);

    /* ------------   Interno   ------------- */
    void ApplyConfig(Enemy enemy)
    {
        /* 1) Awareness -------------------- 
        var aw = enemy.GetComponent<EnemyAwareness>();
        if (aw)
        {
            aw.awarenessRadius = eConfig.awarenessRadius;
            aw.alertDuration   = eConfig.alertDuration;
        }

        2) Attack ----------------------- 
        var atk = enemy.GetComponent<EnemyAttack>();
        if (atk)
        {
            atk.damage     = eConfig.damage;
            atk.cadence = eConfig.cadence;
            atk.range    = eConfig.range;

        }*/

        /* 3) Loot ------------------------- */
        var drop = enemy.GetComponent<EnemyDrop>();
        if (drop)
            drop.ConfigureLoot(eConfig);   
    }
}
