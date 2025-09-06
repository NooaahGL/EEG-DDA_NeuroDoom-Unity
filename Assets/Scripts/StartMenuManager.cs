// StartMenuManager.cs – versión para única escena (splash + menú + pantalla de carga + juego)
// --------------------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MazeGenerator; 


/// Gestiona la introducción del juego cuando TODO está en una sola escena.
/// 1. Muestra un splash estático durante splashDuration segundos.
/// 2. Realiza fade‑out del splash y fade‑in del menú principal.
/// 3. Al pulsar New Game, hace fade‑out del menú, enseña la pantalla de carga
///    (LoadingScreen.Show) e inicia la generación del laberinto.
///    El LoadingScreen se esconderá solo cuando el Generator dispare _onMazeCompleted.

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup _splashGroup;  // Imagen estática
    [SerializeField] private CanvasGroup _menuGroup;    // Panel con botones
    [SerializeField] private GameObject _playerUI;
    [SerializeField] private LoadingScreen _loadingScreen; // Pantalla de carga existente    



    [Header("Timings")]
    [SerializeField] private float _splashDuration = 5f;
    [SerializeField] private float _fadeSpeed      = 1f;

    [Header("Game Systems (single‑scene)")]
    [SerializeField] private Camera _menuCamera; 
    [SerializeField] private Generator _generator;     // Generador del laberinto

    private bool _introFinished;

    /* ---------- Ciclo de vida ---------- */
    void Awake()
    {
        // Comprobaciones rápidas
        if (_splashGroup == null || _menuGroup == null)
            Debug.LogError("StartMenuManager: Faltan referencias de CanvasGroup");
        
        if (_menuCamera != null) _menuCamera.gameObject.SetActive(true);
        MusicManager.Instance?.PlayMenuMusic();
        // Estado inicial de los Canvas
        _splashGroup.alpha        = 1f;
        _splashGroup.blocksRaycasts = false; // no hace falta interactuar

        _menuGroup.alpha          = 0f;  // oculto
        _menuGroup.interactable   = false;
        _menuGroup.blocksRaycasts = false;
    }

    void Start()
    {
        if (_playerUI != null) _playerUI.SetActive(false); 

        StartCoroutine(PlayIntro());
        

        if (_generator != null)
            _generator._onMazeCompleted.AddListener(OnMazeReady);
    }

    /* ---------- Intro & fades ---------- */
    private IEnumerator PlayIntro()
    {   
        
        
        // 1) Aleja el splash el tiempo indicado
        yield return new WaitForSeconds(_splashDuration);

        // 2) Fade‑out splash
        yield return FadeCanvasGroup(_splashGroup, 1f, 0f);

        // 3) Fade‑in menú
        
        yield return FadeCanvasGroup(_menuGroup, 0f, 1f);
        _menuGroup.interactable   = true;
        _menuGroup.blocksRaycasts = true;
        _introFinished            = true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        cg.alpha = from;
        float t  = 0f;
        while (Mathf.Abs(cg.alpha - to) > 0.01f)
        {
            t     += Time.deltaTime * _fadeSpeed;
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        cg.alpha = to;
    }

    /* ---------- Callbacks de botones ---------- */
    public void OnNewGame()
    {
        if (!_introFinished) return; // seguridad
        StartCoroutine(BeginGameSequence());
    }

    public void OnOptions()
    {
        Debug.Log("TODO: abrir panel de opciones");
        // Aquí puedes mostrar un sub‑panel de ajustes.
    }

    public void OnQuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    /* ---------- Comienzo de la partida ---------- */
    private IEnumerator BeginGameSequence()
    {
        // 1) Fade‑out del menú
        _menuGroup.interactable   = false;
        _menuGroup.blocksRaycasts = false;
        yield return FadeCanvasGroup(_menuGroup, _menuGroup.alpha, 0f);

        if (_menuCamera != null) _menuCamera.gameObject.SetActive(false);

        // 2) Mostrar LoadingScreen
        if (_loadingScreen != null) _loadingScreen.Show();

        // 3) Iniciar generación del laberinto
        if (_generator != null)
        {
            // Si tu Generator tiene un método público explícito, llámalo aquí
            _generator.Generate();          // ← Sustituye por el nombre real
        }
        else
        {
            Debug.LogWarning("StartMenuManager: Generator no asignado, no se iniciará el laberinto.");
        }
    }

    private void OnMazeReady()
    {
        MusicManager.Instance?.PlayGameMusic();
        Debug.Log("PlayerUI ACTIVADO");
        if (_playerUI != null)
            _playerUI.SetActive(true);
    }
}
