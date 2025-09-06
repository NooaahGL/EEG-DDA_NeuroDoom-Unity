using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [Header("Configuración")]
    public GameConfig config; 

    private EnemyManager enemyManager;
    private Animator spriteAnim;
    private AngleToPlayer angleToPlayer;


    public float enemyHealth;
    public GameObject gunHitEffect;


    [Header("Audio")]
    public AudioClip deathClip;
    [Range(0f, 1f)] public float deathVolume = 0.5f;


    [Header("Cadáver")]
    public GameObject corpsePrefab ;     

    public UnityEvent onDeath; 

    void Awake(){
        enemyHealth = config.baseEnemyHealth;

    }

    private void Start()
    {
        spriteAnim = GetComponentInChildren<Animator>();
        angleToPlayer = GetComponent<AngleToPlayer>();

        enemyManager = FindObjectOfType<EnemyManager>();
    }

    void Update()
    {
        // 
        spriteAnim.SetFloat("SpriteRot", angleToPlayer.lastIndex);

        if (enemyHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        Instantiate(gunHitEffect, transform.position, Quaternion.identity);
        enemyHealth -= damage;
    }

    void Die()
    {
        ScoreManager.Instance.AddPoints( config.pointsPerKill ); 


        enemyManager.RemoveEnemy(this);
        onDeath.Invoke();

        OneShotAudioFader.Play(deathClip, transform.position, deathVolume, 0.8f, 2f);

        if (corpsePrefab != null)
        {
            Vector3 pos = spriteAnim.transform.position;   // hijo visible
            Quaternion rot = spriteAnim.transform.rotation;   // mantiene orientación
            Instantiate(corpsePrefab, pos, rot);
        } 
        

        Destroy(gameObject);   
    }


}
