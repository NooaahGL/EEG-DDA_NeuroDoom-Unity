using UnityEngine;
using TMPro;             

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText;           

    private float totalScore = 0;
    public float TotalScore => totalScore; 


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // opcional: DontDestroyOnLoad(gameObject);
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = $"Points: {totalScore}";
    }

    /// <summary> Suma puntos y actualiza la UI. </summary>
    public void AddPoints(float points)
    {
        totalScore += points;
        UpdateUI();
    }
}
