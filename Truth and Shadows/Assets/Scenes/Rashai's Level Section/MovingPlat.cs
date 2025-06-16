using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlat : MonoBehaviour
{
    [SerializeField]
    public WaypointPath _waypointPath;

    [SerializeField]
    private float _speed;

    private int _targetWaypointIndex;

    private Transform _previousWayPoint;
    private Transform _targetWayPoint;

    private float _timeToWaypoint;
    private float _elapsedTime;

    void Start()
    {
        TargetMoveWaypoint();
    }

    // Update is called once  per frame
    void FixedUpdate()
    {
        _elapsedTime += Time.deltaTime;

        float elapsedPercentage = _elapsedTime / _timeToWaypoint;
        elapsedPercentage = Mathf.SmoothStep(0, 1, elapsedPercentage);
        transform.position = Vector3.Lerp(_previousWayPoint.position, _targetWayPoint.position, elapsedPercentage);

        if (elapsedPercentage >= 1)
        {
            TargetMoveWaypoint();
        }
    }

    private void TargetMoveWaypoint()
    {
        _previousWayPoint = _waypointPath.GetWaypoint(_targetWaypointIndex);
        _targetWaypointIndex = _waypointPath.GetNextWaypointIndex(_targetWaypointIndex);
        _targetWayPoint = _waypointPath.GetWaypoint(_targetWaypointIndex);

        _elapsedTime = 0;

        float distanceToWaypoint = Vector3.Distance(_previousWayPoint.position, _targetWayPoint.position);
        _timeToWaypoint = distanceToWaypoint / _speed;
    }
}



