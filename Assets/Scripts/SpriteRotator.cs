using UnityEngine;

public class SpriteRotator : MonoBehaviour
{
    private Transform target;


    private Vector3 startPosition;

    [Header("Bobbing")]
    public float bobAmplitude = 0.1f;   // sube/baja
    public float bobFrequency = 0.35f;    //  ciclos por segundo

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var player = FindFirstObjectByType<playerMove>();
        if (player != null) target = player.transform;

        //target = FindObjectOfType<playerMove>().transform;

        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //f (target != null) return;

        // 1) Mirar al jugador (o cámara)
        transform.LookAt(target);

        // 2) Cálculo del offset vertical
        float yOffset = Mathf.Sin(Time.time * bobFrequency * 2f * Mathf.PI)
                        * bobAmplitude;

        // 3) Aplica posición original + offset
        transform.position = startPosition + new Vector3(0, yOffset, 0);

        
    }
}
