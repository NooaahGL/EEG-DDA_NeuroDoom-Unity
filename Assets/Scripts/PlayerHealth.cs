using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{

    [Header("Configuración")]
    public GameConfig config; 

    [Header("Salud")]
    public int maxHealth = 100;

    [Header("Armadura")]
    public int maxArmor = 50;

    [Header("Daño e invulnerabilidad")]
    [Tooltip("Tiempo de invulnerabilidad tras recibir daño (en segundos)")]
    public float invulTime = 0.2f;     // evita daño múltiple por frame

    [Header("Musica")]
    [SerializeField] private AudioSource _audioSource; // Para efectos (golpe, muerte, etc)
    [SerializeField] private AudioClip   _hurtSound;   // Clip para daño

    public static event System.Action OnPlayerDied;


    public int Health => health;
    public int Armor => armor;
    public bool IsDead { get; private set; }


    int health;
    int armor;
    float invulTimer;
 

    public static PlayerHealth Instance { get; private set; }


    void Awake()
    {
        health = config.Init_health ;
        armor = config.Init_armor ;
  

        // Singleton simple 
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        health = maxHealth;
        armor = 0;                // o maxArmor si empezar con armadura

        ActualizarHUD();
    }

    void Start()
    {
        CanvasManager.Instance.UpdateHealth(health);
        CanvasManager.Instance.UpdateArmor(armor);
    }


    void Update()
    {

        if (invulTimer > 0f) invulTimer -= Time.deltaTime;
        // temporary test function
        #if UNITY_EDITOR   // atajo de test, puedes quitarlo en build
                if (Input.GetKeyDown(KeyCode.T)){
                    TakeDamage(30);
                    Debug.Log("Player has been damaged");}
        #endif
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || invulTimer > 0f) return;   // ignora si está muerto o es inmortal 

        invulTimer = invulTime;                  // resetea ventana de invul.

        // 1) Se resta primero a la armadura
        if (armor > 0)
        {
            int absorbido = Mathf.Min(armor, damage);
            armor -= absorbido;
            damage -= absorbido;
        }

        // 2) El resto va a la salud
        if (damage > 0)
            health = Mathf.Max(health - damage, 0);

        ActualizarHUD();
        PlayHurtSound();

        if (health == 0)
            Morir();

    }

    public void PlayHurtSound(){
        if (_hurtSound != null && _audioSource != null)
            _audioSource.PlayOneShot(_hurtSound);
    }



    public void GiveHealth(int amount, GameObject pickup)
    {
        if (IsDead) return;

        if (health >= maxHealth) return;

        health = Mathf.Min(health + amount, maxHealth);

        if (pickup) Destroy(pickup);
        ActualizarHUD();
    }

    public void GiveArmor(int amount, GameObject pickup)
    {
        if (IsDead) return;

        if (armor >= maxArmor) return;

        armor = Mathf.Min(armor + amount, maxArmor);
        if (pickup) Destroy(pickup);
        ActualizarHUD();

    }

    void Morir()
    {
        OnPlayerDied?.Invoke();

        IsDead = true;
        Debug.Log("Player ha muerto");


        MusicManager.Instance?.StopMusic();
        GameOverUI.Instance.Show();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    /// Sincroniza valores con tu Canvas/HUD.
    void ActualizarHUD()
    {
        // Comprueba que el Singleton de Canvas exista por seguridad
        if (CanvasManager.Instance == null) return;

        CanvasManager.Instance.UpdateHealth(health);
        CanvasManager.Instance.UpdateArmor(armor);
    }

}