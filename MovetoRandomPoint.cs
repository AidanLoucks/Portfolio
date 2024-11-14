using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using System.Runtime.CompilerServices;

public class MovetoRandomPoint : Node
{
    private Transform _roomBounds;
    private Transform transform;

    private Vector2 destination;
    private float speed = 5f;

    public MovetoRandomPoint(Transform t, Transform roomBounds)
    {
        _roomBounds = roomBounds;
        transform = t;
        destination = Room.GetPointWithinRoom(_roomBounds);
    }

    public override NodeState Evaluate()
    {
        if (Vector2.Distance(transform.position, destination) < 0.02f)
        {
            destination = Room.GetPointWithinRoom(_roomBounds);

            state = NodeState.Success;
            return state;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        }

        state = NodeState.Running;
        return state;
    }
}
