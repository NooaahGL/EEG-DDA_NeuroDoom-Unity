using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameCOnfig")]
public class GameConfig : ScriptableObject
{
    
    [Header("Dynamic Adaptation (ADD)")]
    [Tooltip("Si está a true, cada horda ajusta la dificultad según emociones.")]
    public AdaptationMode adaptationMode;

    [Header("Hordas")]
    public int startEnemies               = 5;
    public float timeBetweenHordes        = 10f;
    public float timeBetweenSpawns        = 0.3f;
    public float difficultyMultiplier     = 1.2f;// cada horda suben un 50%
    public int maxEnemiesPerHorde         = 50;
    public float baseGrowthPerHorde = 1.15f;


    [Header("Enemigos")]
    public float baseEnemyHealth            = 2f;

    // Atack
    public int attackDamage                 = 10;              // puntos quitados por golpe
    public float attackCadence              = 1f;        // segundos entre golpes
    public float attackRange                = 4f;         // distancia para pegar

    // Awareness
    public float minDistToPlayer            = 3f;  
    public float awarenessRadius            = 100f;
    public float alertDuration              = 8f;
    
    [Header("Labertino")]
    public int mazeWidth                    = 10;
    public int mazeHeight                   = 10;

    [Header("Puntuación")]
    public int pointsPerKill = 10;
    public int pointsPerHorde  = 100;

    [Header("Pickups")]
    public int pickupCount                  = 2;

    [Header("Player")]
    public float playerSpeed                = 10f; 
    public int maxHealth                    = 100;
    public int maxArmor                     = 50;
    public float invulTime                  = 0.2f; // evita daño múltiple por frame

    public int Init_health                  = 100;
    public int Init_armor                   = 0;

    [Header("Gun")]
    public float range                      = 20f;
    public float verticalRange              = 20f;

    public float smallDamage                = 1f;
    public float bigDamage                  = 2f;

    public float fireRate                   = 1f;
    public float gunShotRadius              = 20f;

    public int maxAmmo                     = 25;


}
