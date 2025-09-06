using System.Collections;
using UnityEngine;
using System;
using MazeGenerator;

public class HordeManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config; 


    [Header("Referencias")]
    public MazeManager mazeManager;         // Referencia desde el Inspector
    public EnemyManager enemyManager;       // Referencia desde el Inspector
    public GameObject enemyPrefab;

    [Header("Parámetros de hordas")]
    public int startEnemies;
    float timeBetweenHordes;
    float timeBetweenSpawns;
    float difficultyMultiplier; 
    int maxEnemiesPerHorde;

    int currentHorde = 0;

    public event Action<int> OnHordeStarted; // Para notificar a UI, música, etc.
    public event Action<int> OnHordeCompleted;

    void Awake()
    {
        if (mazeManager == null) mazeManager = FindObjectOfType<MazeManager>();
        if (enemyManager == null) enemyManager = FindObjectOfType<EnemyManager>();

        startEnemies         = config.startEnemies;
        maxEnemiesPerHorde   = config.maxEnemiesPerHorde;
    }

    
    void Start()
    {
        // Lanza la primera horda después de que el Maze esté listo
        mazeManager._generator._onMazeCompleted.AddListener(() => StartCoroutine(StartHordeLoop()));
    }

    IEnumerator StartHordeLoop()
    {
        yield return new WaitForSeconds(1f); // breve espera inicial

        while (true)
        {
            currentHorde++;

            OnHordeStarted?.Invoke(currentHorde);

            
            float growth   = Mathf.Pow(config.baseGrowthPerHorde, currentHorde - 1);
            float adaptive = config.difficultyMultiplier;           // shock/step
            config.difficultyMultiplier = adaptive*growth; // multiplicador de dificultad
            int enemiesToSpawn = Mathf.Min(
                Mathf.RoundToInt(config.startEnemies * growth * adaptive),
                maxEnemiesPerHorde);

            Debug.Log($"-- HORDA {currentHorde} -- Enemigos: {enemiesToSpawn}");
            

            yield return StartCoroutine(SpawnHorde(enemiesToSpawn));

            // Espera a que todos los enemigos estén muertos
            yield return new WaitUntil(() => enemyManager.EnemiesCount == 0);
            OnHordeCompleted?.Invoke(currentHorde);
            Debug.Log("¡Horda completada!");
            
            int score = Mathf.RoundToInt(config.pointsPerHorde * config.difficultyMultiplier);
            ScoreManager.Instance.AddPoints(score);
            Debug.Log($"Bonus por completar horda {currentHorde}: +{score} puntos");

            yield return new WaitForSeconds(config.timeBetweenHordes);
        }
    }

    IEnumerator SpawnHorde(int count)
    {
        var cells = mazeManager.Generator.GetComponent<CellsBuilder>().Cells;
        if (cells == null || cells.Count == 0)
        {
            Debug.LogError("HordeManager: No hay celdas en CellsBuilder");
            yield break;
        }

        // Spawn loop
        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(1, cells.Count);
            Vector3 pos = cells[idx].Position + Vector3.up * 1.0f;

            // Instancia
            var enemyGO = Instantiate(enemyPrefab, pos, Quaternion.identity);
            var enemy   = enemyGO.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("HordeManager: El prefab no tiene componente Enemy");
                continue;
            }

            // REGISTRAR en EnemyManager
            if (enemyManager != null)
                enemyManager.AddEnemy(enemy);
            else
                Debug.LogError("HordeManager: enemyManager es null");

            yield return new WaitForSeconds(config.timeBetweenSpawns);
        }
    }
}
