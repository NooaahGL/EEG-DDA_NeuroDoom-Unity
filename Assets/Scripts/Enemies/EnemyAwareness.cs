using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAwareness : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config; 

    [Header("Distancia mínima al jugador")]
    public float minDistToPlayer = 3f;  // Ajusta según tamaño de tus modelos

    public float awarenessRadius = 8f;
    public float alertDuration = 5f;
    [HideInInspector] public bool isAggro;

    
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float alertTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        minDistToPlayer = config.minDistToPlayer;
        awarenessRadius = config.awarenessRadius;
        alertDuration = config.alertDuration;

    }

    void Start()
    {
        var jugador = FindFirstObjectByType<playerMove>();
        if (jugador != null) playerTransform = jugador.transform;
        else Debug.LogError("No se encontró PlayerMove en la escena");
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- Detección de proximidad ---
        float distancia = Vector3.Distance(transform.position, playerTransform.position);

        bool cerca = distancia <= awarenessRadius;

        if (alertTimer > 0f)
        {
            alertTimer -= Time.deltaTime;
            cerca = true;
        }
        isAggro = cerca;

        // --- Bloqueo por proximidad ---
        if (isAggro)
        {
            if (distancia > minDistToPlayer)
            {
                agent.isStopped = false; // se puede mover
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                agent.isStopped = true; // muy cerca: se detiene
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    public void Alert() => alertTimer = alertDuration;
    
    // Cambia la velocidad del NavMeshAgent durante 'dur' segundos
public void ModifySpeed(float multiplier, float dur)
{
    StartCoroutine(TempSpeedChange(multiplier, dur));
}

IEnumerator TempSpeedChange(float mult, float dur)
{
    float original = agent.speed;
    agent.speed *= mult;
    yield return new WaitForSeconds(dur);
    agent.speed = original;
}

}
