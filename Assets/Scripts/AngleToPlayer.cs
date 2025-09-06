using UnityEngine;

public class AngleToPlayer : MonoBehaviour
{

    private Transform player;
    private Vector3 targetPos;
    private Vector3 targetDir;

    private SpriteRenderer spriteRenderer;

    private float angle;
    public int lastIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var jugador = Object.FindFirstObjectByType<playerMove>();
        if (jugador != null) player = jugador.transform;
        else Debug.LogError("No se encontró PlayerMove en la escena");

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        //player = FindObjectOfType<playerMove>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        // get target position and direction
        targetPos = new Vector3(player.position.x, transform.position.y,player.position.z);
        targetDir = targetPos - transform.position;

        angle = Vector3.SignedAngle(targetDir, transform.forward, Vector3.up);

        Vector3 tempScale = Vector3.one;
        if(angle > 0)
        {
            tempScale.x *= -1f;
        }

        spriteRenderer.transform.localScale = tempScale;

        lastIndex = GetIndex(angle);

    }

    private int GetIndex(float angle){
        // front 
        if(angle > -22.5f && angle < 22.6f)
            return 0;

        if(angle >= 22.5f && angle < 67.5f)
            return 7;

        if(angle >= 67.5f && angle < 112.5f)
            return 6;

        if(angle >= 122.5f && angle < 157.5f)
            return 5;


        // back
        if(angle <= -157.5f || angle >= 157.5f)
            return 4;
        
        if(angle >= -157.4f && angle < -112.5f)
            return 3;
        
        if(angle >= -112.5f && angle < -67.5f)
            return 2;
        
        if(angle >= -67.5f && angle <= -22.5f)
            return 1;
            

        return lastIndex;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetPos);
    }
}
