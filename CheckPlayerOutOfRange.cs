using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerOutOfRange : Node
{
    private Transform playerTransform;
    private Transform transform;
    private float seekRange;

    public CheckPlayerOutOfRange(Transform t, float seekRange)
    {
        transform = t;
        this.seekRange = seekRange;
        playerTransform = GameManager.instance.players[0].transform;
    }

    public override NodeState Evaluate()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) > seekRange)
        {
            state = NodeState.Success;
            return state;
        }
        else
        {
            state = NodeState.Failure;
            return state;
        }
    }
}
