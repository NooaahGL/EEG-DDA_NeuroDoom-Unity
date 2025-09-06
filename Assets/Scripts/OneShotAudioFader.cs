using System.Collections;
using UnityEngine;

public class OneShotAudioFader : MonoBehaviour
{
    /// <summary>
    /// Reproduce un clip 3-D y lo desvanece hasta 0 en fadeTime segundos.
    /// </summary>
    public static void Play(AudioClip clip, Vector3 pos,
                        float startVol = 1f,
                        float fadeTime = 0.6f,
                        float exponent = 1f)   
    {
        if (clip == null) return;

        var go  = new GameObject("SFX_" + clip.name);
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.clip          = clip;
        src.volume        = startVol;
        //src.pitch         = Random.Range(1f - pitchVar, 1f + pitchVar);
        src.spatialBlend  = 1f;                    // sonido 3-D
        src.rolloffMode   = AudioRolloffMode.Linear;
        src.Play();

        go.AddComponent<OneShotAudioFader>()
        .StartCoroutine(FadeAndKill(src, fadeTime, exponent));
    }

    /* ---------- corrutina de fade ---------- */
static IEnumerator FadeAndKill(AudioSource src,
                               float fadeTime, float exponent)
{
    float initVol = src.volume, t = 0f;
    while (t < fadeTime)
    {
        t += Time.deltaTime;
        float pct = Mathf.Pow(t / fadeTime, exponent); // curva
        src.volume = Mathf.Lerp(initVol, 0f, pct);
        yield return null;
    }
    Destroy(src.gameObject);
}
}
