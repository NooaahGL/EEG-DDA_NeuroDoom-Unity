using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("Root del Canvas")]
    [SerializeField] GameObject root;           // arrastra GameOverCanvas aquí

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (root == null) root = gameObject;    // fallback
        root.SetActive(false);                  // empieza oculto
    }

    /* ---------- API ---------- */
    public void Show()
    {
        root.SetActive(true);
        Time.timeScale = 0f;                   // pausa todo
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void Hide()
    {
        Time.timeScale = 1f;
        root.SetActive(false);
    }

    /* ---------- Botones ---------- */
    public void OnRetry()
    {
        Hide();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuit()
    {
        Hide();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;   // detener play mode
        #else
            Application.Quit();
        #endif
    }
}
