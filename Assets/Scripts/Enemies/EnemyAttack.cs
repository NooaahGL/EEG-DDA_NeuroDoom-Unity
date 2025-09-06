using UnityEngine;

[RequireComponent(typeof(EnemyAwareness))]
public class EnemyAttack : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config;
    public LayerMask obstacleMask;    // Capa(s) que bloquean visión (p.ej. muros)
    
    [Header("Apariencia")]
    [Tooltip("Altura desde el suelo donde sale el rayo de visión")]
    public float eyeHeight = 1.2f;

    Transform player;
    float nextAttackTime;

    int damage;
    float cadence;
    float range;

    void Awake()
    {
        // Lee los valores del config
        if (config == null)
        {
            Debug.LogError("EnemyAttack: falta asignar GameConfig");
            enabled = false;
            return;
        }

        damage  = config.attackDamage;
        cadence = config.attackCadence;
        range   = config.attackRange;
    }

    void Start()
    {
        // Cachea la referencia al jugador
        var go = GameObject.FindWithTag("Player");
        if (go == null)
        {
            Debug.LogError("EnemyAttack: no se encontró GameObject con tag Player");
            enabled = false;
            return;
        }
        player = go.transform;
    }

    void Update()
    {
        if (Time.time < nextAttackTime) 
            return;  // aún en cooldown

        Vector3 myPos = transform.position + Vector3.up * eyeHeight;
        Vector3 dir   = (player.position + Vector3.up * eyeHeight) - myPos;
        float  dist  = dir.magnitude;

        // 1) ¿Está a rango?
        if (dist > range) 
            return;

        // 2) ¿Hay obstáculos en el camino?
        if (Physics.Raycast(myPos, dir.normalized, out RaycastHit hit, range, obstacleMask))
        {
            // Si el primer collider que tocas no es el jugador, estás bloqueado
            if (!hit.transform.CompareTag("Player"))
                return;
        }
        // Si no choca con nada o choca con el jugador, seguimos:

        // 3) Atacar
        Attack();
        nextAttackTime = Time.time + cadence;
    }

    void Attack()
    {
        var vida = player.GetComponent<PlayerHealth>();
        if (vida != null)
            vida.TakeDamage(damage);
        // Aquí podrías lanzar efectos de sonido/animación
    }

    // Opcional: para visualizar el rayo en escena
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 forward = transform.forward * range;
        Gizmos.DrawRay(eye, forward);
    }
}
