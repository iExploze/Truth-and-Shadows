using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DirectionalLightDetection : MonoBehaviour
{
    private Light dirLight;

    // track which players are currently 'in sunlight'
    private Dictionary<GameObject, bool> playerSunStates = new Dictionary<GameObject, bool>();

    // ignore anything on the "IgnoreLightRaycast" layer
    private int occlusionMask;

    void Start()
    {
        dirLight = GetComponent<Light>();
        if (dirLight.type != LightType.Directional)
            Debug.LogError("DirectionalLightDetection must be attached to a Directional Light.");

        // everything except IgnoreLightRaycast
        occlusionMask = ~LayerMask.GetMask("IgnoreLightRaycast");
    }

    void Update()
    {
        // find all active players
        var players = GameObject.FindGameObjectsWithTag("Player");
        var stillHere = new HashSet<GameObject>(players);

        // remove any players that have gone away
        foreach (var old in new List<GameObject>(playerSunStates.Keys))
            if (!stillHere.Contains(old))
                playerSunStates.Remove(old);

        // check each player
        foreach (var player in players)
        {
            if (!player.activeInHierarchy) continue;

            var hittable = player.GetComponent<ILightHittable>();
            if (hittable == null) continue;

            bool isInSun = !IsInShadow(player.transform);
            bool wasInSun = playerSunStates.ContainsKey(player) && playerSunStates[player];

            if (isInSun && !wasInSun)
            {
                hittable.OnLightEnter(dirLight);
                playerSunStates[player] = true;
            }
            else if (isInSun && wasInSun)
            {
                hittable.OnLightStay(dirLight);
            }
            else if (!isInSun && wasInSun)
            {
                hittable.OnLightExit(dirLight);
                playerSunStates[player] = false;
            }
            // else: stayed in shadow, do nothing
        }
    }

    private bool IsInShadow(Transform target)
    {
        // cast a ray from the player back toward the sun
        Vector3 rayDir = -transform.forward;
        Vector3 origin = target.position + rayDir * 0.1f;  // nudge outside the collider

        if (Physics.Raycast(origin, rayDir, out RaycastHit hit, 1000, occlusionMask))
        {
            Debug.DrawLine(origin, hit.point, Color.red);
            return true;    // hit something → in shadow
        }
        else
        {
            Debug.DrawRay(origin, rayDir * 10f, Color.green);
            return false;   // nothing hit → in sunlight
        }
    }
}
