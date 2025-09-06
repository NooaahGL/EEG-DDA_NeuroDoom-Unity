using UnityEngine;
using UnityEngine.Events;
using MazeGenerator;          // ⚠️  solo si vas a referenciar al Generator

/// <summary>
/// Muestra un Canvas + cámara provisional y los esconde cuando se complete el laberinto.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Canvas    _canvas;          // Canvas con el texto
    [SerializeField] Camera    _loadingCamera;   // Cámara negra
    [SerializeField] Generator _generator;       // Generador que emitirá el evento

    void Awake()
    {
        // Por si olvidamos asignar algo en el Inspector
        if (_canvas == null)        _canvas = GetComponentInChildren<Canvas>(true);
        if (_loadingCamera == null) _loadingCamera = GetComponentInChildren<Camera>(true);
        if (_generator == null)     _generator = FindObjectOfType<Generator>();

        // Muestra la pantalla desde el principio
        Hide();
        
        _generator._onMazeStarted.AddListener(Show);
        // Se esconderá en cuanto el Generator dispare su evento

        _generator._onMazeCompleted.AddListener(Hide);
    }

    /* ---------- API pública ---------- */
    public void Show()
    {
        _canvas.gameObject.SetActive(true);
        _loadingCamera.enabled = true;
    }

    public void Hide()
    {
        _canvas.gameObject.SetActive(false);
        _loadingCamera.enabled = false;   // o Destroy(_loadingCamera.gameObject);
    }
}
