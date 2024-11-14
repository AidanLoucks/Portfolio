using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public class BossSwitch : Node
    {
        private float waitTime = 2f;
        private float timer;

        public BossSwitch() : base() { }
        public BossSwitch(List<Node> children) : base(children) { }

        private int bossStage = 0;
        public void NextStage()
        {
            bossStage++;
            timer = waitTime;
        }

        public override NodeState Evaluate()
        {
            if (timer <= 0)
            {
                switch (children[bossStage].Evaluate())
                {
                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;
                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;
                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;
                    default:
                        state = NodeState.Success;
                        return state;
                }
            }
            else
            {
                timer -= Time.deltaTime;
                state = NodeState.Running;
                return state;
            }
        }
    }
}
