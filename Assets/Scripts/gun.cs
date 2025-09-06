using UnityEngine;

public class gun : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config; 

    public float range = 20f;
    public float verticalRange = 20f;

    public float smallDamage = 1f;
    public float bigDamage = 2f;

    public float fireRate = 0.7f;
    public float gunShotRadius = 20f;

    private int maxAmmo = 25;
    private int ammo;

    private float nextTimeToFire;
    private BoxCollider gunTrigger;

    public LayerMask raycastLayerMask;
    public LayerMask enemyLayerMask;

    public EnemyManager enemyManager;

    public static event System.Action OnOutOfAmmo;

    bool _notifiedOutOfAmmo = false;
    public int CurrentAmmo => ammo;

    
void Awake()
{
    range                      = config.range ;
    verticalRange              = config.verticalRange ;

    smallDamage                = config.smallDamage ;
    bigDamage                  = config.bigDamage ;

    fireRate                   = config.fireRate ;
    gunShotRadius              = config.gunShotRadius ;

    maxAmmo                     = config.maxAmmo ;


    // 1) Intenta cogerlo del Inspector; si está a null, lo buscamos a mano
    if (enemyManager == null)
        enemyManager = FindFirstObjectByType<EnemyManager>();

    if (enemyManager == null)
        Debug.LogError("[gun] No se encontró EnemyManager en la escena");
}

    void Start()
    {
        gunTrigger = GetComponent<BoxCollider>();
        gunTrigger.size = new Vector3(x:1, y:verticalRange, z: range);
        gunTrigger.center = new Vector3(x:0, y:0, z: range*0.5f);
        ammo =  maxAmmo;
        CanvasManager.Instance.UpdateAmmo(ammo);
    }

    // Update is called once per frame
    void Update()
    {
         if (Input.GetMouseButtonDown(0)&& Time.time > nextTimeToFire && ammo > 0)
         {
            Fire();
            //nextTimeToFire = Time.time + fireRate;
         }
    }

    void Fire()
    {

        // Gun Shot radius

        Collider[] enemyColliders;
        enemyColliders = Physics.OverlapSphere(transform.position, gunShotRadius, enemyLayerMask);

        // alert any enemy in earshot
        foreach (var enemyCollider in enemyColliders)
        {
            var awareness = enemyCollider.GetComponent<EnemyAwareness>();
            if (awareness != null)
                awareness.Alert();
        }

        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().Play();

        foreach (var enemy in enemyManager.enemiesInTrigger)
        {
            if (enemy == null)
            {
                enemyManager.RemoveEnemy(enemy);    // límpialo y continúa
                continue;
            }
            // get direction to enemy
            Vector3 dir = enemy.transform.position - transform.position;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, direction: dir, out hit, maxDistance: range * 1.5f, raycastLayerMask))
            {
                if (hit.transform == enemy.transform)
                {
                    // range check
                    float distance = Vector3.Distance(enemy.transform.position, b: transform.position);
                    if (distance > range * 0.5f)
                    {
                        enemy.TakeDamage(smallDamage);
                    }
                    else
                    {
                        enemy.TakeDamage(bigDamage);
                    }

                    //Debug.DrawRay(start:transform.position, dir, Color.green);
                    //Debug.Break();
                }
            }

        }

        //reset timer
        nextTimeToFire = Time.time + fireRate;

        // deduct ammo
        ammo--;
        CanvasManager.Instance.UpdateAmmo(ammo);
        
        if (ammo == 0 && !_notifiedOutOfAmmo)
        {
            _notifiedOutOfAmmo = true;      // evita spam de eventos
            OnOutOfAmmo?.Invoke();
        }
    }

    public bool GiveAmmo(int amount, GameObject pickup)
    {
        if (ammo >= maxAmmo) return false;

        ammo = Mathf.Min(ammo + amount, maxAmmo);
        Destroy(pickup);

        CanvasManager.Instance.UpdateAmmo(ammo);
        
        _notifiedOutOfAmmo = false;          // ya no estamos “secos”
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //add potential enemy to shoot
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy){
            enemyManager.AddEnemy(enemy);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        //remove potential enemy to shoot
        Enemy enemy = other.transform.GetComponent<Enemy>();

        if (enemy){
            enemyManager.RemoveEnemy(enemy);
        }
    }
}
