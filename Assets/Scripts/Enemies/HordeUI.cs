using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(HordeManager))]
public class HordeUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI messageText;   // Arrástralo en el Inspector
    public float          showDuration = 2f;

    private HordeManager  hordeManager;

    void Awake()
    {
        hordeManager = GetComponent<HordeManager>();
        // Oculto por defecto
        messageText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        hordeManager.OnHordeStarted   += HandleHordeStarted;
        hordeManager.OnHordeCompleted += HandleHordeCompleted;  // opcional
    }

    void OnDisable()
    {
        hordeManager.OnHordeStarted   -= HandleHordeStarted;
        hordeManager.OnHordeCompleted -= HandleHordeCompleted;
    }

    private void HandleHordeStarted(int hordeNumber)
    {
        StartCoroutine(ShowMessageCoroutine($"Wave {hordeNumber} started!", showDuration));
    }

    private void HandleHordeCompleted(int hordeNumber)
    {
        // Si quisieras avisar al acabar:
        //StartCoroutine(ShowMessageCoroutine($"Wave {hordeNumber} completed!", showDuration));
    }

    private IEnumerator ShowMessageCoroutine(string msg, float duration)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }
}
