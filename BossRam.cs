using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class BossRam : Node
    {
        private Transform bossTransform;
        private Vector2 target;

        private float speed = 12f;
        private bool hasStartedRam = false;

        public BossRam(Transform t) { bossTransform = t; }

        public override NodeState Evaluate()
        {
            // Cache the inital player position once
            if (!hasStartedRam)
            {
                target = GameManager.instance.players[0].transform.position;
                hasStartedRam = true;
            }

            // Ram towards the player
            if (Vector2.Distance(bossTransform.position, target) > 0.01f)
            {
                // Attack
                bossTransform.position = Vector2.MoveTowards(bossTransform.position, target, speed * Time.deltaTime);
            }
            else // When it reaches that original position, return success
            {
                hasStartedRam = false;
                return NodeState.Success;
            }
            state = NodeState.Running;
            return state;
        }
    }
}

