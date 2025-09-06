using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("Prefabs de botín")]
    [SerializeField] GameObject healthPickup, smallHealthPickup;
    [SerializeField] GameObject ammoPickup, smallAmmoPickup;
    [SerializeField] GameObject armorPickup, smallArmorPickup;

    [Header("Probabilidades")]
    [Range(0, 1)] public float dropChance = 0.4f;      // 40 % de soltar algo
    [Range(0, 1)] public float ammoWeight = 0.5f;    // 50 % dentro del 40 %
    [Range(0, 1)] public float armorWeight = 0.2f;    // 20 %
    [Range(0, 1)] public float healthWeight = 0.3f;    // 30 %

    //[HideInInspector]
    [Range(0, 1)]  public float smallProb    = 0.5f;

    void Awake()
    {
        // Suscríbete al evento de muerte
        GetComponent<Enemy>().onDeath.AddListener(HandleDeath);
    }

    public void ConfigureLoot(EnemyConfig cfg)          // ← NUEVO
    {
        dropChance   = cfg.dropChance;
        ammoWeight   = cfg.ammoWeight;
        armorWeight  = cfg.armorWeight;
        healthWeight = cfg.healthWeight;
        smallProb    = cfg.smallProb;
    }

    void HandleDeath()
    {
        if (Random.value > dropChance) return;

        GameObject prefab = ChoosePickup();
        if (prefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 0.4f;
        Instantiate(prefab, pos, Quaternion.identity);
    }

    GameObject ChoosePickup()
    {
        float roll = Random.value;     // [0,1)
        if (roll < ammoWeight)                    // 0.00 – 0.49 → munición
            return PickSize(ammoPickup, smallAmmoPickup);

        if (roll < ammoWeight + armorWeight)      // 0.50 – 0.69 → armadura
            return PickSize(armorPickup, smallArmorPickup);

        /* 0.70 – 0.99 → salud (lo que resta) */
        return PickSize(healthPickup, smallHealthPickup);
    }
    
    GameObject PickSize(GameObject big, GameObject small)
    {
        // Si no definiste versión pequeña devolverá la grande
        bool smallRequested = Random.value < smallProb && small != null;
        return smallRequested ? small : big;
    }
}
