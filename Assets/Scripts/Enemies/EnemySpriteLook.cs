using UnityEngine;

public class EnemySpriteLook : MonoBehaviour
{
    private Transform target;
    public bool canLookVertically;

    void Start()
    {
        
        target = FindFirstObjectByType<playerMove>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (canLookVertically)
        {
            transform.LookAt(target);
        }
        else
        {
            Vector3 modifiedTarget = target.position;
            modifiedTarget.y = transform.position.y;

            transform.LookAt(modifiedTarget);
        }
        
    }
}
