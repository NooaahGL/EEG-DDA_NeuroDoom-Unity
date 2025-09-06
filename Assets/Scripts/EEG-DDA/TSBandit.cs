using System;
using System.IO;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;
using Newtonsoft.Json;
using UnityEngine;

public enum BrainState { Flow, Stress, Bored, Relax, Neutral }   // 0-4

// =============================================================
// TSBandit — Thompson Sampling (Linear Contextual Bandit)
// Compatible con CONTEXT_DIM = 10 definido en EmotionTracker.
// =============================================================
public class Context
{
    public double[] x;
    public string ToCsv() => string.Join(",", x);
}

// =============================================================
// TSBandit — Thompson Sampling para bandido lineal bayesiano
// -------------------------------------------------------------
// * Mantiene para cada brazo k una posterior 𝓝(μ_k, Σ_k)
// * Al muestrear: θ_k ~ 𝓝(μ_k, Σ_k); se escoge argmax θ_k·x
// * Σ_k se almacena directamente (A^{-1}); calculamos y cacheamos
//   su Cholesky para acelerar el muestreo.
// =============================================================

public class TSBandit
{
    public const int EXPECTED_DIM = 10;
    class Arm
    {
        public Matrix<double> Sigma;  // Σ_k  (A_k⁻¹)
        public Vector<double> Mu;     // μ_k
        public Matrix<double> L;      // Cholesky(Σ_k) ⇒ Σ = L Lᵀ
    }
    private Arm[] arms;
    private readonly MersenneTwister rng = new MersenneTwister();

    // =============================================================
    // CARGA DESDE JSON
    // =============================================================
    public void Load(string path)
    {
        if (!File.Exists(path)) { Debug.LogWarning("[TSBandit] Archivo no encontrado: " + path); return; }

        // Carga el JSON y deserializa
        var raw = JsonConvert.DeserializeObject<Wrapper>(File.ReadAllText(path));
        if (raw?.Sigma == null || raw.mu == null)
        {
            Debug.LogError($"[TSBandit] JSON sin matrices Sigma/mu: {path}");
            return;
        }

        int k = raw.Sigma.Length;
        int dim = raw.mu[0].Length;
        if (dim != EXPECTED_DIM)
        {
            Debug.LogWarning($"[TSBandit] Dimensión en JSON ({dim}) ≠ EXPECTED_DIM ({EXPECTED_DIM}) → ignorado, usar InitPrior");
            return;
        }

        arms = new Arm[k];

        for (int i = 0; i < k; i++)
        {
            var Sigma = Matrix<double>.Build.DenseOfRowArrays(raw.Sigma[i]);
            var mu = Vector<double>.Build.Dense(raw.mu[i]);
            var L = SafeCholesky(Sigma);// Cholesky de Σ (ya SPD)

            arms[i] = new Arm
            {
                Sigma = Sigma,   
                Mu = mu,
                L = L
            };
        }
    }
    public void InitPrior(int nArms, int dim = EXPECTED_DIM, double priorVar = 1.0)
    {
        arms = new Arm[nArms];
        var Sigma0 = Matrix<double>.Build.DenseIdentity(dim) * priorVar;
        var L0     = SafeCholesky(Sigma0);

        for (int k = 0; k < nArms; k++)
        {
            arms[k] = new Arm
            {
                Sigma = Sigma0.Clone(),
                Mu    = Vector<double>.Build.Dense(dim),
                L     = L0.Clone()
            };
        }
    }

    // =============================================================
    // SELECCIÓN DE BRAZO POR THOMPSON SAMPLING
    // =============================================================
    public int SampleArm(Context ctx)
    {
        if (arms == null || arms.Length == 0) { Debug.LogError("[TSBandit] Modelo no inicializado"); return 0; }
        if (ctx.x.Length != arms[0].Mu.Count) { Debug.LogError($"[TSBandit] Context dim {ctx.x.Length} ≠ modelo {arms[0].Mu.Count}"); return 0; }
        // Fallback: flow (0)

        var v = Vector<double>.Build.DenseOfArray(ctx.x);
        double bestScore = double.NegativeInfinity;
        int bestArm = 0;

        for (int idx = 0; idx < arms.Length; idx++)
        {
            Arm arm = arms[idx];

            // θ = μ + L·N(0,I)
            var noise = Vector<double>.Build.Dense(
                            arm.Mu.Count,
                            _ => Normal.Sample(rng, 0, 1)); 
            var theta = arm.Mu + arm.L * noise;

            double score = theta.DotProduct(v);
            if (score > bestScore)
            {
                bestScore = score;
                bestArm = idx;
            }
        }
        return bestArm;
    }

    private static Matrix<double> SafeCholesky(Matrix<double> M, double jitter = 1e-8)
    {
        try { return M.Cholesky().Factor; }
        catch (Exception)
        {
            // Añadimos jitter a la diagonal hasta que sea SPD
            var eye = Matrix<double>.Build.DenseIdentity(M.RowCount);
            double eps = jitter;
            while (eps < 1e-2)
            {
                try { return (M + eps * eye).Cholesky().Factor; }
                catch { eps *= 10; }
            }
            // Como último recurso devolvemos la raíz de la identidad
            Debug.LogWarning("[TSBandit] Cholesky falló; usando identidad");
            return eye;
        }
    }

    // =============================================================
    // SERIALIZACIÓN A JSON
    // =============================================================
    [System.Serializable]
    class Wrapper
    {
        public double[][][] Sigma;   // [arm][row][col]
        public double[][] mu;   // [arm][row]
    }
}
