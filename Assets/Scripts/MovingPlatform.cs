using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform[] wayPoints;
    public float moveSpeed = 5f;
    public Vector2 difference;

    private Vector3 _lastPosition;
    private Vector3 _currentWayPoint;
    private int waypointCounter;

    
    void Start()
    {
        waypointCounter = 0;
        _currentWayPoint = wayPoints[waypointCounter].position;
    }

   
    void Update()
    {
        _lastPosition = transform.position;

        transform.position = Vector3.MoveTowards(transform.position, _currentWayPoint, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position,_currentWayPoint)<0.1f)
        {
            waypointCounter++;

            if(waypointCounter>=wayPoints.Length)
            {
                waypointCounter = 0;
            }
            _currentWayPoint = wayPoints[waypointCounter].position;
        }

        difference = transform.position - _lastPosition;
    }
}
