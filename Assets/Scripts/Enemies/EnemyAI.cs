using UnityEngine.AI;
using System;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    private EnemyAwareness enemyAwareness;
    private Transform playersTransform;
    private UnityEngine.AI.NavMeshAgent enemyNavMeshAgent;

    void Start()
    {
        enemyAwareness = GetComponent<EnemyAwareness>();
        //playersTransform = FindObjectOfType<playerMove>().transform;

        var playerMove     = FindFirstObjectByType<playerMove>();
        if (playerMove != null)
            playersTransform = playerMove.transform;
        else
            Debug.LogError("No se encontró PlayerMove en la escena");

        enemyNavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }


    void Update()
    {
        if (playersTransform == null) return;

        if (enemyAwareness.isAggro){        
            enemyNavMeshAgent.SetDestination(playersTransform.position);
        }else{
            // Vuelve a su antigua posición
            enemyNavMeshAgent.SetDestination(transform.position);
            // Se detiene en el lugar 
            //enemyNavMeshAgent.ResetPath();
        }
    }
}
