using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// =============================================================
// EmotionTracker — ajuste de dificultad mediante Thompson Sampling
// =============================================================

[RequireComponent(typeof(UdpMindwaveReceiver))]
public class EmotionTracker : MonoBehaviour
{
    // ---------- Constantes ----------
    private const int CONTEXT_DIM = 10;   // nº total de features en el vector x
    private const int N_ARMS      = 4;    // Flow, Stress, Bored, Relax
    private const int QUEUE_LEN   = 100;   // ventana para medias móviles

    // ---------- Configuración Unity ----------
    [Header("Configuración")]
    public GameConfig config; 

    [Header("Referencias")]
    public UdpMindwaveReceiver receiver;
    public HordeManager        hordeManager;

    // ---------- Umbrales EEG ----------
    private int attH = 60, medH = 60;
    private int attL = 40, medL = 40;

    
    // ---------- Buffers y Estado ----------
    private readonly List<UdpMindwaveReceiver.EsensePacket> history = new();
    private readonly List<string> hordeEmotionHistory = new();
    private readonly Queue<float> attQ = new();
    private readonly Queue<float> medQ = new();


    private int killsThisHorde;
    private int dominantIdxThisHorde = (int)BrainState.Neutral;
    private int previousEmotionIndex = (int)BrainState.Neutral;
    private string lastEmotion = null;



    #region === ML-Bandit ===
    [Header("ML Bandit")]
    public string banditModelPath = "Assets/StreamingAssets/bandit.json";
    public string banditCsvPath   = "Assets/StreamingAssets/logs.csv";
    private Context ctxPrevHorde;   // X^{H-1}
    private int     predPrevState = -1;    // p̂_H
    private TSBandit bandit;                 // predictor


    // Copias para restaurar al salir
    float bs, bh, bd;       // Base Spawn, base horde and base difficulty

    #endregion

    // =============================================================
    // CICLO DE VIDA
    // =============================================================

    void Awake()
    {
        if (receiver == null) receiver = GetComponent<UdpMindwaveReceiver>();
        if (hordeManager == null) hordeManager = FindObjectOfType<HordeManager>();

        // Respaldo de valores base
        bs = config.timeBetweenSpawns;      // Base Spawn
        bh = config.timeBetweenHordes;      // Base Horde 
        bd = config.difficultyMultiplier;  // Base Diff

        bandit = new TSBandit();
        Directory.CreateDirectory(Path.GetDirectoryName(banditCsvPath));

        if (File.Exists(banditModelPath))
        {
            bandit.Load(banditModelPath);
            Debug.Log($"[Bandit] Modelo cargado: {banditModelPath}");
        }
        else
        {
            bandit.InitPrior(N_ARMS, CONTEXT_DIM);
            Debug.Log($"[Bandit] Prior inicializada ({CONTEXT_DIM} dims)");
        }        

        //Suscribe a eventos
        receiver.OnPacketReceived += OnPacket;
        hordeManager.OnHordeStarted += OnHordeStart;
        hordeManager.OnHordeCompleted += OnHordeEnd;

    }

    void OnDestroy()
    {
        receiver.OnPacketReceived   -= OnPacket;
        hordeManager.OnHordeStarted   -= OnHordeStart;
        hordeManager.OnHordeCompleted -= OnHordeEnd;
    }


    // =============================================================
    // DATOS EEG
    // =============================================================
    void OnPacket(UdpMindwaveReceiver.EsensePacket pkt)
    {
        history.Add(pkt);

        // ---- actualiza medias móviles y detecta el estado “instantáneo” ----
        if (attQ.Count >= 40) attQ.Dequeue();
        if (medQ.Count >= 40) medQ.Dequeue();
        attQ.Enqueue(pkt.attention);
        medQ.Enqueue(pkt.meditation);

    }

    // =============================================================
    // HORDA INICIO/FIN
    // =============================================================
    void OnHordeStart(int hordeNumber)
    {
        history.Clear();

        float growth   = Mathf.Pow(config.baseGrowthPerHorde, hordeNumber - 1);
        float adaptive = config.difficultyMultiplier;

        killsThisHorde = Mathf.RoundToInt(
            hordeManager.startEnemies * growth * adaptive);
    }

    void OnHordeEnd(int hordeNumber)
    {
        var (dominant, domIdx) = ComputeDominantEmotion();
        dominantIdxThisHorde   = domIdx;
        RecordAndAdapt(dominant, hordeNumber);
    }

    private (string emotion, int idx) ComputeDominantEmotion()
    {
        if (history.Count == 0) return (BrainState.Neutral.ToString(), (int)BrainState.Neutral);

        int cFlow = 0, cStress = 0, cRelax = 0, cBored = 0, cNeutral = 0;

        foreach (var p in history)
        {
            bool attHigh = p.attention > attH;
            bool attLow = p.attention < attL;
            bool medHigh = p.meditation > medH;
            bool medLow = p.meditation < medL;

            if      (attHigh && medHigh)    cFlow++;
            else if (attHigh && medLow)     cStress++;
            else if (attLow && medHigh)     cRelax++;
            else if (attLow && medLow)      cBored++;
            else                            cNeutral++;
        }

        int max = Mathf.Max(Mathf.Max(cFlow, cStress), Mathf.Max(cRelax, cBored));
        int idx = max == cFlow   ? 0 :
                  max == cStress ? 1 :
                  max == cBored  ? 2 :
                  max == cRelax  ? 3 : 4;

        return (((BrainState)idx).ToString(), idx);
    }

    // =============================================================
    // ADAPTACIÓN
    // =============================================================
    void RecordAndAdapt(string realEmotion, int hordeNumber)
    {
        hordeEmotionHistory.Add(realEmotion);
        Debug.Log($"[EmotionTracker] Horda {hordeNumber} → emoción dominante: {realEmotion}");

        // -------- Contexto de la horda que acaba (H) --------
        Context ctxCurrent = BuildContext();
        int realIdx = (int)Enum.Parse(typeof(BrainState), realEmotion, true);

        // -------- Calcula recompensa para la decisión previa --------
        if (ctxPrevHorde != null && predPrevState != -1)
        {
            double pPred = GetPropByIdx(ctxCurrent, predPrevState); // proporción del estado PREDICHO en H
            double pReal = GetPropByIdx(ctxCurrent, realIdx);       // proporción del estado REAL dominante en H
            double reward = 1.0 - Math.Abs(pPred - pReal);           // ∈ [0,1]

            LogBanditCsv(ctxPrevHorde, predPrevState, realIdx, reward);
        }

        // -------- Predicción para la próxima horda (H+1) ------------
        int predNextState = bandit.SampleArm(ctxCurrent);
        string predNextEmotion = ((BrainState)predNextState).ToString();

        // -------- Guarda para la siguiente vuelta -------------------
        ctxPrevHorde = ctxCurrent;
        predPrevState = predNextState;
        

        // -------- Ajustes in-game -----------------------------------
        if (hordeNumber == 1) return;        // nada que ajustar en la primera

        switch (config.adaptationMode)
        {
            case AdaptationMode.Preconfigured:
                AdaptPreconfigured(realEmotion);
                break;

            case AdaptationMode.HeuristicTree:
                AdaptHeuristicTree(realEmotion, previous: lastEmotion);
                break;

            case AdaptationMode.ML:
                Debug.Log($"[Bandit] Predice → {predNextEmotion} para horda {hordeNumber}");
                AdaptHeuristicTree(predNextEmotion, previous: lastEmotion);
                break;
        }

        lastEmotion = realEmotion;

    }

    #region Estrategias de adaptación

    void AdaptPreconfigured(string emotion)
    {
        // Mantengo las condiciones igual a las iniciales del GameConfig
    }

    void AdaptHeuristicTree(string emotion, string previous)
    {

        var maze = hordeManager.mazeManager;
        bool stateChanged = emotion != previous;

        switch (emotion)
        {
            case "Flow":
                ApplyMultipliers(stateChanged
                    ? (1.20f, 0.85f, 0.85f)   // SHOCK
                    : (1.10f, 0.97f, 0.97f)); // STEP
                break;

            case "Stress":
                ApplyMultipliers(stateChanged
                    ? (0.8f, 1.25f, 1.50f)
                    : (0.7f, 1.10f, 1.20f));
                maze.SpawnPickups(config.pickupCount * 2);
                break;

            case "Bored":
                ApplyMultipliers(stateChanged
                    ? (1.10f, 0.90f, 1.00f)   // horde gap sin cambio
                    : (1.30f, 0.75f, 1.00f));
                foreach (var ag in FindObjectsOfType<NavMeshAgent>())
                    ag.speed *= stateChanged ? 1.3f : 1.2f;
                maze.SpawnPickups(config.pickupCount);
                break;

            case "Relax":
                ApplyMultipliers(stateChanged
                    ? (1.10f, 1.05f, 1.00f)
                    : (1.20f, 0.95f, 1.00f));
                break;
        }

        // ----- 2 · límites de seguridad -----
        config.difficultyMultiplier = Mathf.Clamp(config.difficultyMultiplier, 0.5f, 3.5f);
        config.timeBetweenSpawns = Mathf.Clamp(config.timeBetweenSpawns, 0.25f, 5.0f);
        config.timeBetweenHordes = Mathf.Clamp(config.timeBetweenHordes, 3.0f, 10.0f);

        Debug.Log($"[Heuristic–{emotion}] stateChanged={stateChanged}  " +
                $"diff={config.difficultyMultiplier:0.00}  " +
                $"spawn={config.timeBetweenSpawns:0.00}s  " +
                $"horde={config.timeBetweenHordes:0.0}s");
                
        void ApplyMultipliers((float diff, float spawn, float horde) m)
        {
            config.difficultyMultiplier *= m.diff;
            config.timeBetweenSpawns   *= m.spawn;
            config.timeBetweenHordes   *= m.horde;
        }
    }

    #endregion
    void LogBanditCsv(Context ctx, int predState, int realIdx, double reward)
    {
        string features = string.Join(",", ctx.x.Select(v => v.ToString(CultureInfo.InvariantCulture)));
        string line = $"{Time.frameCount},{features},{predState},{realIdx},{reward.ToString(CultureInfo.InvariantCulture)}";
        File.AppendAllText(banditCsvPath, line + Environment.NewLine);
    }

    private static double GetPropByIdx(Context ctx, int armIdx) => ctx.x[6 + armIdx];

    // =============================================================
    // CONTEXT BUILD
    // =============================================================
    Context BuildContext()
    {
        float avgAtt = attQ.Count > 0 ? attQ.Average() : 0f;
        float avgMed = medQ.Count > 0 ? medQ.Average() : 0f;

        int total = history.Count;

        int cFlow=0,cStress=0,cBored=0,cRelax=0,cNeutral=0;
        foreach (var p in history)
        {
            bool attHigh = p.attention > attH;
            bool attLow  = p.attention < attL;
            bool medHigh = p.meditation > medH;
            bool medLow  = p.meditation < medL;
            if(attHigh&&medHigh)        cFlow++;
            else if (attHigh && medLow) cStress++;
            else if (attLow && medHigh) cRelax++;
            else if (attLow && medLow)  cBored++;
            else                        cNeutral++; }

        // Evita división por cero
        if (cFlow + cStress + cBored + cRelax == 0) cFlow = 1;

        total = history.Count - cNeutral;

        double pFlow    = total>0?(double)cFlow/total:0;
        double pStress  = total>0?(double)cStress/total:0;
        double pBored   = total>0?(double)cBored/total:0;
        double pRelax   = total>0?(double)cRelax/total:0;

        // Construye el vector de contexto
        // (diff, stateNow, kills, EEG(2), prevIdx, session, pFlow, pStress, pBored, pRelax, pNeutral)

        //Objetivo: Poner todas las features en un rango comparable (≈ −1 … +1) 
        // cada peso θᵢ empiece con la misma varianza (priorVar = 1.0),
        // las matrices A y Σ tengan buen condicionamiento,
        // y el muestreo θ_k ~ 𝓝(μ_k, Σ_k) de bandir sea más rápido y estable.

        // escala lineal a ±1
        double diff    = (config.difficultyMultiplier - 2.25) / 1.75;
        double dDom = 0;//((double)dominantIdxThisHorde - 1.5) / 1.5;
        double kills = 0;//((double)killsThisHorde / config.maxEnemiesPerHorde) * 2.0 - 1.0;
        double aAtt    = (avgAtt - 50.0) / 50.0;
        double aMed    = (avgMed - 50.0) / 50.0;
        double pIdx    = (previousEmotionIndex - 2.0) / 2.0;
        double fFlow   = (pFlow   - 0.5) / 0.5;
        double fStress = (pStress - 0.5) / 0.5;
        double fBored  = (pBored  - 0.5) / 0.5;
        double fRelax  = (pRelax  - 0.5) / 0.5;

        double[] x = {
            diff,         // 0
            dDom,         // 1
            kills,        // 2
            aAtt,         // 3
            aMed,         // 4
            pIdx,         // 5
            fFlow,        // 6
            fStress,      // 7
            fBored,       // 8
            fRelax        // 9
        };

        previousEmotionIndex = dominantIdxThisHorde;
        return new Context { x = x };
    }

    void OnApplicationQuit()
    {

        config.timeBetweenSpawns = bs;      // Base Spawn
        config.timeBetweenHordes = bh;      // Base Horde 
        config.difficultyMultiplier = bd;  // Base Diff


        string msg = "=== Historial de emociones por horda ===\n";
        for (int i = 0; i < hordeEmotionHistory.Count; i++)
        {
            msg += $"Horda {i + 1}: {hordeEmotionHistory[i]}\n";
        }

        if (ScoreManager.Instance != null)
            msg += $"=== Puntuación final: {ScoreManager.Instance.TotalScore} puntos ===";

        Debug.Log(msg);

        }
}
