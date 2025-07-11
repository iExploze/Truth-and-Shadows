using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class PointLightDetection : MonoBehaviour
{
    private Light pointLight;
    private Dictionary<GameObject, bool> playerLightStates = new Dictionary<GameObject, bool>();
    private int occlusionMask;

    void Start()
    {
        pointLight = GetComponent<Light>();
        if (pointLight.type != LightType.Point)
            Debug.LogError("PointLightDetection must be attached to a Point Light.");

        // everything except IgnoreLightRaycast
        occlusionMask = ~LayerMask.GetMask("IgnoreLightRaycast");
    }

    void Update()
    {
        // find all active players
        var players = GameObject.FindGameObjectsWithTag("Player");
        var stillHere = new HashSet<GameObject>(players);

        // clean up any gone-away players
        foreach (var old in new List<GameObject>(playerLightStates.Keys))
            if (!stillHere.Contains(old))
                playerLightStates.Remove(old);

        // check each player
        foreach (var player in players)
        {
            if (!player.activeInHierarchy) continue;

            var hittable = player.GetComponent<ILightHittable>();
            if (hittable == null)
            {
                Debug.LogWarning($"No ILightHittable on {player.name}");
                continue;
            }

            bool isInLight = IsInPointLight(player.transform);
            bool wasInLight = playerLightStates.ContainsKey(player) && playerLightStates[player];

            if (isInLight && !wasInLight)
            {
                hittable.OnLightEnter(pointLight);
                playerLightStates[player] = true;
            }
            else if (isInLight && wasInLight)
            {
                hittable.OnLightStay(pointLight);
            }
            else if (!isInLight && wasInLight)
            {
                hittable.OnLightExit(pointLight);
                playerLightStates[player] = false;
            }
            // else: stayed out of light → nothing
        }
    }

    private bool IsInPointLight(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        // only consider within the light's range
        if (distance > pointLight.range)
            return false;

        // cast toward the player
        Ray ray = new Ray(transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, pointLight.range, occlusionMask))
        {
            // if the first thing hit is the player → in light
            if (hit.transform == target)
            {
                Debug.DrawLine(transform.position, hit.point, Color.green);
                return true;
            }
            else
            {
                Debug.DrawLine(transform.position, hit.point, Color.red);
                return false;
            }
        }

        // nothing in between → in light
        Debug.DrawRay(transform.position, direction * 10f, Color.green);
        return true;
    }
}
