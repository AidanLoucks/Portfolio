using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class BossPatrol : Node
{
    private Transform bossTransform;
    private Transform[] waypoints;

    private int currentWaypointIndex;

    private float speed = 5f;

    public BossPatrol(Transform t, Transform[] wp)
    {
        this.waypoints = wp;
        bossTransform = t;
    }

    public override NodeState Evaluate()
    {
        // Get the next Waypoint
        Transform wp = waypoints[currentWaypointIndex];

        // If the boss is at the waypoint, move to the next one
        if (Vector2.Distance(bossTransform.position, wp.position) < 0.02f)
        {
            bossTransform.position = wp.position;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            state = NodeState.Success;
            return state;
        }
        // if not, move towards it
        else
        {
            bossTransform.position = Vector2.MoveTowards(bossTransform.position, wp.position, speed * Time.deltaTime);
        }

        state = NodeState.Running;
        return state;

    }

}
