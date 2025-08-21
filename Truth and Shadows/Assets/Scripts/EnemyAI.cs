using TruthAndShadows.CheckpointSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public LayerMask obstructionMask;

    private NavMeshAgent agent;
    private int patrolIndex;
    [SerializeField] private Transform playerTransform;
    private StateManager playerState;
    private RagdollOnOff playerRagdoll;

    private bool chasing = false;
    private bool noticed = false;
    private float noticedTimer = 0f;
    [SerializeField] private float noticedDuration = 2.5f;
    [SerializeField] private AudioSource chaseMusicSource;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 7f;

    [SerializeField] private float distanceToKill = 3f;

    private Light spotLight;
    private Color originalSpotColor;
    private float originalIntensity;

    public Color noticedColor = Color.red;
    public float noticedIntensityMultiplier = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolIndex = 0;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        agent.speed = patrolSpeed;

        playerState = FindAnyObjectByType<StateManager>();
        playerRagdoll = FindAnyObjectByType<RagdollOnOff>();

        spotLight = GetComponent<Light>();

        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
        }
        else
        {
            originalSpotColor = spotLight.color;
            originalIntensity = spotLight.intensity;
        }
    }

    void Update()
    {
        bool canSee = canSeePlayer();
        bool humanForm = playerState.isHumanForm();

        // Handle Noticed State
        if (canSee && humanForm && !chasing)
        {
            if (!noticed)
            {
                noticed = true;
                noticedTimer = 0f;
            }
            else
            {
                noticedTimer += Time.deltaTime;
                // Lerp color/intensity
                float t = Mathf.Clamp01(noticedTimer / noticedDuration);
                spotLight.color = Color.Lerp(originalSpotColor, noticedColor, t);
                spotLight.intensity = Mathf.Lerp(originalIntensity, originalIntensity * noticedIntensityMultiplier, t);

                if (noticedTimer >= noticedDuration)
                {
                    chasing = true;
                    noticed = false;

                    agent.speed = chaseSpeed;
                    // Start chase music
                    if (chaseMusicSource != null && !chaseMusicSource.isPlaying)
                        chaseMusicSource.Play();
                }
            }
        }
        else if (noticed && (!canSee || !humanForm))
        {
            // Reset noticed state & restore spotlight
            ResetNotice();
        }

        // Handle chasing
        if (chasing)
        {
            if (canSee && humanForm)
            {
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                chasing = false;

                agent.speed = patrolSpeed;

                // Return to patrol after chase
                patrolIndex = GetNearestPatrolPointIndex();
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
                // Stop chase music
                if (chaseMusicSource != null && chaseMusicSource.isPlaying)
                    chaseMusicSource.Stop();
                // Restore light
                ResetSpotlight();
            }
        }
        else if (!noticed)
        {
            // Patrol as usual
            if (!agent.pathPending && agent.remainingDistance < 0.2f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            // Restore spotlight if not in noticed
            ResetSpotlight();
        }

        // Check if close enough to "catch" the player
        if (chasing && Vector3.Distance(transform.position, playerTransform.position) < distanceToKill && canSee)
        {
            // Instead of reloading the scene, use the CheckpointManager to respawn at checkpoint
            if (CheckpointManager.Instance != null)
            {
                // Use the CheckpointManager's HandleEnemyKill method
                //CheckpointManager.Instance.HandleEnemyKill();
                StartCoroutine(KillPlayerSequence());
            }
            else
            {
                // Fallback to scene reload if CheckpointManager isn't available
                Debug.LogWarning("CheckpointManager not found, falling back to scene reload");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    private IEnumerator KillPlayerSequence()
    {
        //Trigger ragdoll if available
        if (playerRagdoll != null)
        {
            //playerRagdoll.SendMessage("RagdollModeOn", SendMessageOptions.DontRequireReceiver);
            playerRagdoll.RagdollModeOn();
        }

        //Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        //Trigger ragdoll if available
        if (playerRagdoll != null)
        {
            playerRagdoll.RagdollModeOff();
        }

        //Respawn player
        CheckpointManager.Instance.HandleEnemyKill();

    }

    void ResetNotice()
    {
        noticed = false;
        noticedTimer = 0f;
        ResetSpotlight();
    }

    void ResetSpotlight()
    {
        if (spotLight != null)
        {
            spotLight.color = originalSpotColor;
            spotLight.intensity = originalIntensity;
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
        return isPlayerInCone(playerTransform);
    }

    private bool isPlayerInCone(Transform target)
    {
        if (spotLight == null || spotLight.type != LightType.Spot)
        {
            Debug.LogError("This script requires a Spot Light assigned.");
            return false;
        }
        Vector3 directionToPlayer = (target.position - spotLight.transform.position).normalized;
        float angleToPlayer = Vector3.Angle(spotLight.transform.forward, directionToPlayer);
        if (angleToPlayer <= spotLight.spotAngle / 2)
        {
            float distanceToPlayer = Vector3.Distance(spotLight.transform.position, target.position);
            if (distanceToPlayer <= spotLight.range)
            {
                int layerMask = ~LayerMask.GetMask("IgnoreLightRaycast");
                Ray ray = new Ray(spotLight.transform.position, directionToPlayer);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, spotLight.range, layerMask))
                {
                    if (hit.transform == target)
                    {
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.green);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(spotLight.transform.position, hit.point, Color.red);
                        return false;
                    }
                }
            }
        }
        return false;
    }
}
