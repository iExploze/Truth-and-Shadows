using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstructionMask;

    private NavMeshAgent agent;
    private int patrolIndex;
    [SerializeField]private Transform playerTransform;
    private StateManager playerState;
    private bool chasing = false;
    private float lastSeenTime;
    [SerializeField] private AudioSource chaseMusicSource;


    private Light spotLight;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolIndex = 0;
        agent.SetDestination(patrolPoints[patrolIndex].position);

        playerState = FindAnyObjectByType<StateManager>();
        spotLight = GetComponent<Light>();

        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }
    }

    void Update()
    {
        bool canSee = canSeePlayer();
        bool humanForm = playerState.isHumanForm();

        Debug.Log("Can see player: " + canSee);

        if (canSee && humanForm)
        {
            Debug.Log("chasing");
            // Chase!
            agent.SetDestination(playerTransform.position);
            chasing = true;
            lastSeenTime = Time.time;

            // Play chase music if not already playing
            if (chaseMusicSource != null && !chaseMusicSource.isPlaying)
                chaseMusicSource.Play();
        }
        else if (chasing && (!humanForm || !canSee))
        {
            chasing = false;
            // Find nearest patrol point index
            patrolIndex = GetNearestPatrolPointIndex();
            // Advance to next patrol point for patrolling
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);

            if (chaseMusicSource != null && chaseMusicSource.isPlaying)
                chaseMusicSource.Stop();
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

        // Check if close enough to "catch" the player
        if (Vector3.Distance(transform.position, playerTransform.position) < 3.5f && canSee)
        {
            // Reset the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    int GetNearestPatrolPointIndex()
    {
        float minDist = Mathf.Infinity;
        int closestIndex = 0;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float d = Vector3.Distance(playerTransform.position, patrolPoints[i].position);
            if (d < minDist)
            {
                minDist = d;
                closestIndex = i;
            }
        }
        return closestIndex;
    }



    private bool canSeePlayer()
    {
        Debug.Log("testing");
        return isPlayerInCone(playerTransform);
    }

    // Helper function to check if a target Transform is within the spotlight's cone
    private bool isPlayerInCone(Transform target)
    {
        // Safety check in case the light is missing or not a spotlight
        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
            return false;
        }

        // Get the direction from spotlight to player
        Vector3 directionToPlayer = (target.position - spotLight.transform.position).normalized;

        // Calculate the angle between the spotlight's forward direction and the direction to the player
        float angleToPlayer = Vector3.Angle(spotLight.transform.forward, directionToPlayer);

        // Check if the player is within the light cone's angle
        if (angleToPlayer <= spotLight.spotAngle / 2)
        {
            // Calculate the distance between the spotlight and the player
            float distanceToPlayer = Vector3.Distance(spotLight.transform.position, target.position);
            // Check if the player is within the spotlight's range
            if (distanceToPlayer <= spotLight.range)
            {
                int layerMask = ~LayerMask.GetMask("IgnoreLightRaycast");

                // Cast a ray in the calculated direction and check for a hit
                Ray ray = new Ray(spotLight.transform.position, directionToPlayer);
                RaycastHit hit;

                // Check if the ray hits the player and is not blocked
                if (Physics.Raycast(ray, out hit, spotLight.range, layerMask))
                {

                    // only return true if the VERY FIRST thing hit is the target itself
                    if (hit.transform == target)
                    {
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.green);
                        return true;
                    }
                    else
                    {
                        // hit something else (another player or an obstacle) first
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.red);
                        return false;
                    }
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
            float d = Vector3.Distance(playerTransform.position, p.position);
            if (d < minDist)
            {
                minDist = d;
                closest = p.position;
            }
        }
        return closest;
    }
}
