using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Qué")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Cuándo")]
    [SerializeField] private float intervalo = 3f;
    [SerializeField] private int   maxVivos  = 8;

    /* ---- runtime ---- */
    private readonly List<Transform> puntosSpawn = new();
    private float t;

    /* ---------------------------------------------------- */
    private void Awake()
    {
        /* Rellena la lista con los hijos que tengan el tag SpawnPoint */
        foreach (Transform tr in GetComponentsInChildren<Transform>(true))
            if (tr.CompareTag("SpawnPoint"))
                puntosSpawn.Add(tr);
    }

    private void OnEnable()  => t = 0f;   // resetea cada vez que el segmento se reactiva
    private void Update()
    {
        t += Time.deltaTime;
        if (t < intervalo) return;

        if (EnemigosActivos() < maxVivos && puntosSpawn.Count > 0)
            InstanciarEnemigo();

        t = 0f;
    }

    /* ---------------- helpers ---------------- */
    private void InstanciarEnemigo()
    {
        Transform p = puntosSpawn[Random.Range(0, puntosSpawn.Count)];
        Instantiate(enemyPrefab, p.position, p.rotation, transform);  // hijo del segmento
    }

    private int EnemigosActivos()
    {
        // OJO: usa esto solo como MVP; luego cambia a pooling + contador
        return GameObject.FindGameObjectsWithTag("Enemy").Length;
    }
}
