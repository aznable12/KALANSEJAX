using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class npcFactory : MonoBehaviour {

    public List<GameObject> NPCModels = new List<GameObject>();
    public Transform center;                  // Center of area on which to randomly place NPCs.
    public float envRange = 1000f;            // Radius of area on which to randomly place NPC.
    private float seekFraction = 0.05f;       // The size of seekRange relative to envRange.
    private float seekRange;                  // The distance from a random point to search to find a NavMesh point.
    public Transform NPCparent;               // Parent game object under which to put factory-created NPCs.
    public float usualSpeed = 1.0f;           // Default usual speed.
    public float usualSpeedVariation = 0.2f;  // Amount plus/minus that speed can vary from usual speed.

    Animator NPCAnimator;
    int maxDepth = 10;

    void Awake()
    {
        seekRange = envRange * seekFraction;
    }

    public void AddNPCtemplate()
    {
        // Add a new index position to the end of our list
        NPCModels.Add(null);
    }

    public void RemoveNPCtemplate(int index)
    {
        //Remove an index position from our list at a point in our list array
        NPCModels.RemoveAt(index);
    }

    public Vector3 randomPointOnNavMesh(int depth)
    {
        Vector3 thePoint = Vector3.zero;
        // Choose a random 2D point within range.
        Vector2 randomOffset2 = Random.insideUnitCircle;
        Vector3 randomOffset3;
        randomOffset3.x = randomOffset2[0];
        randomOffset3.y = 0f;
        randomOffset3.z = randomOffset2[1];
        Vector3 randomPoint = center.position + envRange * randomOffset3;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, seekRange, NavMesh.AllAreas))
        {
            thePoint = hit.position;
        }
        else if (depth < maxDepth)
        {
            // No valid point found.  Try again.
            thePoint = randomPointOnNavMesh(depth+1);
        }
        return thePoint;
    }

    public GameObject CreateNPC()
    {
        GameObject newNPC;

        Vector3 pos = randomPointOnNavMesh(0);
        int randomModel = Random.Range(0, NPCModels.Count);
        Quaternion rot = Quaternion.Euler(0f, 360f * Random.value, 0f);
        if (NPCparent)
            newNPC = (GameObject)Instantiate(NPCModels[randomModel], pos, rot, NPCparent);
        else
            newNPC = (GameObject)Instantiate(NPCModels[randomModel], pos, rot);

        // Add an npcIndividual script to the NPC.  That in turn pulls in the NavMeshAgent component.
        newNPC.AddComponent<npcIndividual>();
        npcIndividual indiv = newNPC.GetComponent<npcIndividual>();

        // Assign a random avoidance priority.
        NavMeshAgent nma = newNPC.GetComponent<NavMeshAgent>();
        if (nma == null)
            Debug.LogError("New NPC does not have a NavMeshAgent.");
        else
            nma.avoidancePriority = Mathf.FloorToInt(Random.Range(0f, 99f));

        // Set the usual speed in the states script, with a small variation.
        indiv.usualSpeed = usualSpeed + Random.Range(-usualSpeedVariation, usualSpeedVariation);

        return newNPC;
    }


}
