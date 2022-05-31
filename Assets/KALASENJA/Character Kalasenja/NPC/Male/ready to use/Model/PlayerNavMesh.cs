using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerNavMesh : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;

    [SerializeField] private Transform movePositionTransform;
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
     
    private void Update() 
    {
        navMeshAgent.destination = movePositionTransform.position;
    }
}
