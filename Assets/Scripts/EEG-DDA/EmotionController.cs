using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [Header("Flow Settings")]
    //[SerializeField] private float slowMoScale = 0.4f;
    //[SerializeField] private float slowMoDuration = 2.5f;


    [Header("Stress Settings")]
    [SerializeField, Range(0f, 1f)] private float enemyHpCut = 0.35f;


    [Header("Relax Settings")]
    [SerializeField] private float speedMultiplier = 1.6f;
    [SerializeField] private float boredomDuration = 6f;


    [Header("Boredom Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Vector3 bossOffset = new Vector3(0, 0, 12);


    void Update()
    {
#if UNITY_EDITOR // accesos rápidos sólo en el editor
        if (Input.GetKeyDown(KeyCode.M)) HandleEmotion("flow"); // Nada
        if (Input.GetKeyDown(KeyCode.N)) HandleEmotion("estres"); // Disminuye salud
        if (Input.GetKeyDown(KeyCode.V)) HandleEmotion("relajacion"); // Aumenta la velocidad de los enemigos
        if (Input.GetKeyDown(KeyCode.B)) HandleEmotion("aburrimiento"); // Crea enemigos
#endif
    }

    public void HandleEmotion(string emotion)
    {
        switch (emotion)
        {
            case "flow":           Flow(); break;
            case "estres":         Stress(); break;
            case "relajacion":     Relax(); break;
            case "aburrimiento":   Boredom(); break;
            default:
                Debug.LogWarning($"[EmotionController] emocion desconocida: {emotion}"); 
                break;
        }
    }

    #region Acciones concretas
    void Flow()
    {
        Debug.Log("Emocion: Flow");
        // Nada aun
    }

    void Stress()
    {
        Debug.Log("Emocion: Stress");
        
        foreach (var enemy in FindObjectsOfType<Enemy>())
        {

            float tentativeDmg = enemy.enemyHealth * enemyHpCut;
            // Queremos que siempre quede un mínimo de vida (>0) para no matar al enemigo
            const float minRemaining = 0.1f; // puedes ajustar
            float dmg = Mathf.Min(tentativeDmg, enemy.enemyHealth - minRemaining);

            if (dmg > 0f) enemy.TakeDamage(dmg);
        }

    }

    void Relax()
    {

        Debug.Log("Emocion: Relax");

        foreach (var aw in FindObjectsOfType<EnemyAwareness>())
            aw.ModifySpeed(speedMultiplier, boredomDuration);

    }

    void Boredom()
    { 
        Debug.Log("Emocion: Aburrimiento");

        if (bossPrefab == null)
        {
            Debug.LogError("bossPrefab no asignado, se omite el spawn");
            return;
        }
        Transform player = FindPlayer();
        if (player != null)
            Instantiate(bossPrefab, player.position + bossOffset, Quaternion.identity);

    }
    #endregion

    void ResetTime() => Time.timeScale = 1f;

    Transform FindPlayer() => GameObject.FindGameObjectWithTag("Player")?.transform;
}
