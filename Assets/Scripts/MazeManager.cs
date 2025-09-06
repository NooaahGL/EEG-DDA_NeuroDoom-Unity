using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;
using MazeGenerator;

public class MazeManager : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config;

    [Header("Generador y NavMesh")]
    [SerializeField] public Generator _generator;
    [SerializeField] NavMeshSurface _navSurface;

    [Header("Prefabs")]
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _enemyPrefab;

    [Header("Pickups")]
    [SerializeField] GameObject[] _pickupPrefabs;
    [SerializeField, Min(0)] int _pickupCount;

    [SerializeField] GameObject _ammoPickupPrefab;
    int _maxAmmoPickups = 2;
    int _currentAmmoPickups = 0;


    [Header("Ajustes")]
    [SerializeField] int _enemyCount = 5;
    public Generator Generator => _generator;

    void Awake()
    {
        _pickupCount = config.pickupCount;
        _generator._onMazeCompleted.AddListener(BuildNavMeshAndSpawn);

        gun.OnOutOfAmmo += HandleOutOfAmmo;
        ItemPickup.OnAmmoPickupCollected += HandleAmmoCollected;
    }

    void OnDestroy()
    {
        // Buen hábito: desuscribirse en caso de que la escena se recargue
        gun.OnOutOfAmmo -= HandleOutOfAmmo;
        ItemPickup.OnAmmoPickupCollected -= HandleAmmoCollected;
    }

    void HandleOutOfAmmo()
    {
        if (_currentAmmoPickups < _maxAmmoPickups)
            SpawnAmmoPickup();
    }
    void HandleAmmoCollected()
    {
        _currentAmmoPickups = Mathf.Max(0, _currentAmmoPickups - 1);
    }

    void Start()
    {
        Debug.Log("MazeManager Start → llamo a Generate");
        //_generator.Generate();
    }

    void BuildNavMeshAndSpawn()
    {
        /* Asegúrate de que las paredes están en su sitio y marcadas como Navigation Static
           (Si tu prefab de pared ya trae BoxCollider y la capa correcta, puedes omitir el bucle) */
        foreach (var wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            //if (!wall.TryGetComponent(out Collider _))
            //wall.AddComponent<BoxCollider>();      // rápido por si faltaba
        }

        // 1) Hornea la malla
        _navSurface.BuildNavMesh();
        Debug.Log("NavMesh horneado");

        // 2) Ahora sí, instanciamos personajes
        SpawnPlayer();
        //SpawnEnemies();

        SpawnPickups();

    }


    void SpawnPlayer()
    {
        // --- Jugador en la primera celda ---
        var firstCell = _generator.GetComponent<CellsBuilder>().Cells[0];
        Vector3 playerPos = firstCell.Position + Vector3.up * 1.8f;

        GameObject player = Instantiate(_playerPrefab, playerPos, Quaternion.identity);
        player.name = "Player";


    }

    void SpawnEnemies()
    {
        // --- Enemigos en celdas aleatorias ---
        var cells = _generator.GetComponent<CellsBuilder>().Cells;
        for (int i = 0; i < _enemyCount; i++)
        {
            var randomCell = cells[Random.Range(1, cells.Count)];
            Vector3 enemyPos = randomCell.Position + Vector3.up * 1.0f;
            Instantiate(_enemyPrefab, enemyPos, Quaternion.identity);
        }

        // (Opcional) Notificar a UI, música, etc.
        Debug.Log("¡Lab y personajes listos!");

    }

    public void SpawnPickups(int count)
    {
        int prev = _pickupCount;
        _pickupCount = count;
        SpawnPickups();      // llamas al método original
        _pickupCount = prev; // restauras
    }

    void SpawnPickups()
    {
        if (_pickupPrefabs == null || _pickupPrefabs.Length == 0)
        {
            Debug.LogWarning("[MazeManager] No hay pickups asignados");
            return;
        }

        var cells = _generator.GetComponent<CellsBuilder>().Cells;
        var nav = _navSurface;  // tu NavMeshSurface

        HashSet<int> usedIndices = new() { 0 };           // evita la celda del jugador

        for (int i = 0; i < _pickupCount; i++)
        {
            // elige una celda libre
            int idx;
            do { idx = Random.Range(1, cells.Count); }
            while (!usedIndices.Add(idx));                // sale cuando añade un índice nuevo

            Vector3 cellPos = cells[idx].Position;
            // muévete un poco arriba para samplear
            Vector3 sampleOrigin = cellPos + Vector3.up * 5f;

            // busca la posición más cercana en el NavMesh
            if (NavMesh.SamplePosition(sampleOrigin, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                // lo pones unos centímetros sobre el suelo
                Vector3 spawnPos = hit.position + Vector3.up * 0.1f;
                var prefab = _pickupPrefabs[Random.Range(0, _pickupPrefabs.Length)];
                Instantiate(prefab, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"MazeManager: no encontré NavMesh cerca de {cellPos}");
            }
        }
    }
    
    void SpawnAmmoPickup()
    {
        if (_ammoPickupPrefab == null)
        {
            Debug.LogWarning("[MazeManager] Falta asignar _ammoPickupPrefab");
            return;
        }

        var cells = _generator.GetComponent<CellsBuilder>().Cells;
        var nav = _navSurface;

        // Busca una celda aleatoria (evitando la 0, donde suele estar el jugador)
        for (int intentos = 0; intentos < 50; intentos++)
        {
            int idx = Random.Range(1, cells.Count);
            Vector3 cellPos = cells[idx].Position + Vector3.up * 5f;

            // Intenta encontrar un punto válido en el NavMesh
            if (NavMesh.SamplePosition(cellPos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position + Vector3.up * 0.1f;
                Instantiate(_ammoPickupPrefab, spawnPos, Quaternion.identity);
                Debug.Log("[MazeManager] Spawned AMMO pickup");
                return;
            }
        }

        Debug.LogWarning("[MazeManager] No pude colocar un pickup de munición tras varios intentos");
    }


}
