using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[ExecuteInEditMode]
public class npcIndividual : MonoBehaviour {

    // Waypoint variables
    public GameObject waypointParent; // Waypoints should be under a common parent in the Scene.
    public float range = 1000f;       // Distance for finding waypoints.
    public bool limitRange = false;

    // Configurable speed values.
    public float usualSpeed = 1f;

    // The other variables are private and used for internal calculations.

    Animator anim;
    NavMeshAgent agent;

    // Parameters related to synchronization between navigation and animation.
    // These are not public so that the user is overwhelmed with parameters that usually don't need to be adjusted.
    float moveTheshold = 0.05f;  // For speeds above this, the NPC will move rather than idle.
    float synchThreshold = 0.1f; // Multiplier for agent radius, offset greater than this between agent and animator will pull the animator to the agent.
    float synchDamping = 0.1f;   // Parameter that controls the rate of pulling the animator to the agent.
    float smoothTime = 0.1f;     // For calls to SmoothDamp() and SmoothDampAngle().
    float currentVelocity;
    float agentSpeed;

    // Variables related to the NavMeshAgent.
    Vector3 currentPosition;
    Vector3 nextPosition;
    Vector3 deltaPosition;
    Vector3 currentForward;
    Vector3 nextForward;
    float prevDeltaForward;
    float currDeltaForward;
    Vector2 agentVelocity;
    float dxAgent, dyAgent;
    float vxAgent, vyAgent;
    Vector2 nextPosition2D;

    // Variables related to the Animator.
    Vector3 currAnimPosition;
    Vector2 currAnimPosition2D;

    // Variable related to the offset between the NavMeshAgent and the Animator.
    Vector2 agentAnimOffset;
    float agentAnimOffsetMagnitude;
    float currentSpeed = 0f;
    float targetSpeed = 0f;
    float smoothDampVelocity;

    Vector3 invalidVector = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

    // state machine variables
    string curState;
    Vector3 destination = Vector3.zero; // Initialize to an arbitrary value.
    public delegate void enterStateFunction();
    public delegate void updateStateFunction();
    private bool debugFSM = false;

    public class npcState
    {
        public string stateName;
        public enterStateFunction enterStateFn;
        public updateStateFunction updateStateFn;
        public npcState(string nm, enterStateFunction enterfn, updateStateFunction updatefn)
        {
            stateName = nm;
            enterStateFn = enterfn;
            updateStateFn = updatefn;
        }
    }

    public npcState[] myStates;

    public npcIndividual()
    {
        myStates = new npcState[]
        {
            new npcState("Start",           enterStartState,           updateStartState),
            new npcState("Travel",          enterTravelState,          updateTravelState),
            new npcState("Paused",          enterPausedState,          updatePausedState),
            new npcState("NewDestination",  enterNewDestinationState,  updateNewDestinationState)
        };
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!anim) Debug.LogError(this.name + " could not get Animator.");
        // Set the NPC's controller.
        string ctrlName = "UnityStandardClipsController";
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(ctrlName);
        if (controller == null)
            Debug.LogError("The Animator Controller " + ctrlName + " could not be loaded.");
        anim.runtimeAnimatorController = controller;

        agent = GetComponent<NavMeshAgent>();
        if (!agent) Debug.LogError(this.name + " could not get the NavMeshAgent.");
        if (agent.stoppingDistance == 0.0f)
        {
            agent.stoppingDistance = 0.5f;
        }

        if (waypointParent == null)
        {
            waypointParent = GameObject.Find("Waypoints");
            if (waypointParent == null)
                Debug.LogError("Need to define some waypoints.");
        }
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        if (!anim) Debug.LogError(this.GetType() + " could not get Animator.");
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (!agent) Debug.LogError(this.GetType() + " could not get the NavMeshAgent.");
        agent.updatePosition = false;
        agentVelocity = Vector2.zero;
        nextPosition = currentPosition = transform.position;
        nextForward = currentForward = transform.forward;
        currAnimPosition = transform.position;
        prevDeltaForward = currDeltaForward = 0f;
        enterNewDestinationState();
    }

    void Update()
    {
        // Some variables related to the next position, thanks to the NavMeshAgent.
        currentPosition = nextPosition;
        nextPosition = agent.nextPosition;
        deltaPosition = nextPosition - currentPosition;
        dxAgent = Vector3.Dot(transform.right, deltaPosition);
        dyAgent = Vector3.Dot(transform.forward, deltaPosition);
        vxAgent = dxAgent / Time.deltaTime;
        vyAgent = dyAgent / Time.deltaTime;
        agentVelocity.x = vxAgent; agentVelocity.y = vyAgent;
        agentSpeed = agentVelocity.magnitude;

        currentForward = nextForward;
        nextForward = transform.forward; // Transform of the NPC GameObject rather than the NavMeshAgent.
        prevDeltaForward = currDeltaForward;
        currDeltaForward = Vector3.Angle(nextForward, currentForward);
        if (Vector3.Cross(nextForward, currentForward).y > 0f)
        {
            currDeltaForward = -currDeltaForward;
        }
        // Smooth out deltaAgentForward.
        currDeltaForward = Mathf.SmoothDampAngle(prevDeltaForward, currDeltaForward, ref currentVelocity, smoothTime);

        if (agentSpeed > moveTheshold)
        {
            anim.SetBool("move", true);
            anim.SetFloat("velx", currDeltaForward);
            anim.SetFloat("vely", vyAgent);
        }
        else
        {
            anim.SetBool("move", false);
        }

        // Determine whether the animator and agent have drifted too far apart.
        currAnimPosition = transform.position;
        nextPosition2D.x = nextPosition.x;
        nextPosition2D.y = nextPosition.z;
        currAnimPosition2D.x = currAnimPosition.x;
        currAnimPosition2D.y = currAnimPosition.z;
        agentAnimOffset = nextPosition2D - currAnimPosition2D;
        agentAnimOffsetMagnitude = agentAnimOffset.magnitude;
        if (agentAnimOffsetMagnitude > agent.radius * synchThreshold)
        {
            // Pull the animator to the agent.
            Vector2 animTarget = currAnimPosition2D + agentAnimOffset;  // same as nextPosition2D
            Vector2 newAnimPosition2D = Vector2.Lerp(currAnimPosition2D, animTarget, synchDamping);
            Vector3 newAnimPosition;
            newAnimPosition.x = newAnimPosition2D.x;
            newAnimPosition.y = transform.position.y;
            newAnimPosition.z = newAnimPosition2D.y;
            transform.position = newAnimPosition;
        }

        // Other functions to call every Update.
        FSMUpdate();
        adjustSpeed();
    }

    void OnAnimatorMove()
    {
        // Match the y (vertical) coordinate of the animator and the agent.
        Vector3 position = anim.rootPosition;
        position.y = agent.nextPosition.y;
        transform.position = position;
    }


    private void FSMUpdate()
    {
        // Find the entry in myStates for curState, and call its update function.
        bool matchFound = false;
        for (int i = 0; i < myStates.Length; i++)
        {
            if (curState == myStates[i].stateName)
            {
                myStates[i].updateStateFn();
                matchFound = true;
                break;
            }
        }
        if (!matchFound)
        {
            Debug.LogError("Unrecognized state: " + curState);
        }
    }

    // Waypoint code.
    public Vector3 getAWaypoint(Vector3 omit, float angMin = 0f, float angMax = 90f)
    {
        List<Vector3> validWPlist = new List<Vector3>();  // Randomly choose one of the valid waypoints.
        List<Vector3> tooFarWPlist = new List<Vector3>();  // If there are no valid waypoints, randomly choose one of the others.
        List<Vector3> wrongDirWPlist = new List<Vector3>();
        int cnt = waypointParent.transform.childCount;
        Vector3 selfPos;
        Vector3 selfForward;
        Vector3 returnVector = invalidVector;
        selfPos = transform.position;
        selfForward = transform.forward;
        // Examine all the waypoints.
        for (int i = 0; i < cnt; i++)
        {
            Vector3 currentWP = waypointParent.transform.GetChild(i).position;
            // Is the current waypoint the one that should be omitted?
            if (currentWP == omit) continue;
            // Is the current waypoint too far away?
            if (limitRange)
            {
                float dist = Vector3.Distance(selfPos, currentWP);
                if (dist > range)
                {
                    tooFarWPlist.Add(currentWP);
                    continue;
                }
            }
            // Is the current waypoint in the wrong direction?
            float angleToWaypoint = Vector3.Angle(selfForward, currentWP - selfPos);
            if (angleToWaypoint < angMin || angleToWaypoint > angMax)
            {
                wrongDirWPlist.Add(currentWP);
                continue;
            }
            // If we made it to here, the waypoint is valid.
            validWPlist.Add(currentWP);
        }
        // If there are any valid waypoints, choose one at random.
        if (validWPlist.Count > 0)
        {
            int index = (int)Mathf.Floor(Random.value * validWPlist.Count);
            returnVector = validWPlist[index];
        }
        else if (wrongDirWPlist.Count > 0)
        {
            // Consider a point that is in the wrong direction.
            int index = (int)Mathf.Floor(Random.value * wrongDirWPlist.Count);
            returnVector = wrongDirWPlist[index];
        }
        else if (tooFarWPlist.Count > 0)
        {
            // Consider a point that is too far.
            int index = (int)Mathf.Floor(Random.value * tooFarWPlist.Count);
            returnVector = tooFarWPlist[index];
        }
        else
        {
            // Should never get here unless there is only one waypoint -- the one being omitted.
            Debug.LogError("Need to have more than one waypoint defined.");
        }
        return returnVector;
    }

    /*
     * Every state XXX needs functions enterXXXState(), updateXXXState, and a case in leaveState().
     * By convention, enterXXXState() should start with a call to leaveState().
     */

    public void enterStartState()
    {
        //leaveState() is not called for the initial state.
        curState = "Start";
        if (debugFSM) Debug.Log(string.Format("{0}: entering", curState));
    }

    private void updateStartState()
    {
        enterNewDestinationState();
    }

    public void enterPausedState()
    {
        leaveState();
        curState = "Paused";
        setSpeed(0f);
        if (debugFSM) Debug.Log(string.Format("{0}: entering", curState));
    }

    private void updatePausedState()
    {
        enterNewDestinationState();
    }

    public void enterTravelState()
    {
        leaveState();
        curState = "Travel";
        setSpeed(usualSpeed);
        if (debugFSM) Debug.Log(string.Format("{0}: entering", curState));
    }

    private void updateTravelState()
    {
        if (getDistanceToDestination() < agent.stoppingDistance)
            enterPausedState();
    }

    public void enterNewDestinationState()
    {
        leaveState();
        curState = "NewDestination";
        setSpeed(0f);
        Vector3 newDestination = getAWaypoint(destination);
        setDestination(newDestination);
        if (debugFSM) Debug.Log(string.Format("{0}: entering", curState));
    }

    private void updateNewDestinationState()
    {
        enterTravelState();
    }

    protected virtual void leaveState()
    {
        switch (curState)
        {
            case "Start":
                {
                    break;
                }
            case "Paused":
                {
                    break;
                }
            case "Travel":
                {
                    break;
                }
            case "NewDestination":
                {
                    break;
                }
        }
        if (debugFSM) Debug.Log(string.Format("{0}: Leaving ", curState));
    }

    /*
     * Some utility functions related to movement.
     */
    public void setSpeed(float newTargetSpeed, bool force = false)
    {
        targetSpeed = newTargetSpeed;
        if (force)
        {
            agent.speed = currentSpeed = targetSpeed;
        }
    }

    // Gradually change speed to targetSpeed.
    void adjustSpeed()
    {
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref smoothDampVelocity, smoothTime);
        agent.speed = currentSpeed;
    }

    public void setDestination(Vector3 newDest)
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.destination = newDest;
            destination = newDest;
        }
    }

    public float getDistanceToDestination()
    {
        if (Application.isPlaying)  // Not while executing in editor.
        {
            float agentDistance = agent.remainingDistance;
            if (agentDistance < Mathf.Infinity && agentDistance >= 0f)
                return agentDistance;
        }
        return Vector3.Distance(transform.position, destination);
    }

}
