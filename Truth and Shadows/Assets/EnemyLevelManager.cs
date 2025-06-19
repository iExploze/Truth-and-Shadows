using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLevelManager : MonoBehaviour
{
    [Header("Objects to Watch")]
    public List<GameObject> watchedObjects;

    [Header("To Disable When Triggered")]
    public List<GameObject> toDisable;

    [Header("To Enable When Triggered")]
    public List<GameObject> toEnable;

    private bool triggered = false;

    void Update()
    {
        if (!triggered && AllWatchedObjectsDisabled())
        {
            TriggerStateChange();
            triggered = true;
        }
    }

    private bool AllWatchedObjectsDisabled()
    {
        foreach (GameObject obj in watchedObjects)
        {
            if (obj != null && obj.activeSelf)
                return false;
        }
        return true;
    }

    private void TriggerStateChange()
    {
        foreach (GameObject obj in toDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        foreach (GameObject obj in toEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}
