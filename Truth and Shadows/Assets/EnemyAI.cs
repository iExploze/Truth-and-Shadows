using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask playerMask, obstructionMask;

    private NavMeshAgent agent;
    private int patrolIndex;
    private Transform player;
    private StateManager playerState;
    private bool chasing = false;
    private float lastSeenTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolIndex = 0;
        agent.SetDestination(patrolPoints[patrolIndex].position);

        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerState = player.GetComponent<StateManager>();
    }

    void Update()
    {
        bool canSee = CanSeePlayer();
        bool humanForm = playerState.isHumanForm();

        if (canSee && humanForm)
        {
            // Chase!
            agent.SetDestination(player.position);
            chasing = true;
            lastSeenTime = Time.time;
        }
        else if (chasing && (!humanForm || !canSee))
        {
            // Stop chase instantly if form is lost or LOS is lost
            chasing = false;
            agent.SetDestination(GetNearestPatrolPoint());
        }
        else if (!chasing)
        {
            // Patrol as usual
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }
    }


    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) < viewAngle / 2)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer < viewDistance)
            {
                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, obstructionMask))
                {
                    // LOS
                    return true;
                }
            }
        }
        return false;
    }

    Vector3 GetNearestPatrolPoint()
    {
        float minDist = Mathf.Infinity;
        Vector3 closest = patrolPoints[0].position;
        foreach (var p in patrolPoints)
        {
            float d = Vector3.Distance(player.position, p.position);
            if (d < minDist)
            {
                minDist = d;
                closest = p.position;
            }
        }
        return closest;
    }
}
