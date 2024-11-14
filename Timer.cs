using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    // can you create a timer node that when it recives a success, it waits for a certain amount of time before evaluating its child node again?

    public class Timer : Node
    {
        public float waitTime;
        private float timer;

        public Timer(float waitTime) : base() { this.waitTime = waitTime; }

        public Timer(Node child, float waitTime) : base(child) 
        { 
            this.waitTime = waitTime;
        }

        public override NodeState Evaluate()
        {
            if (timer <= 0)
            {
                // This foreach is unnessesary. Timer will only ever have one child.
                foreach (Node child in children)
                {
                    switch (child.Evaluate())
                    {
                        case NodeState.Failure:
                            state = NodeState.Failure;
                            return state;
                        case NodeState.Success:
                            state = NodeState.Success;
                            timer = waitTime;
                            return state;
                        case NodeState.Running:
                            state = NodeState.Running;
                            return state;
                        default:
                            state = NodeState.Success;
                            return state;
                    }
                }
            }
            else
            {
                timer -= Time.deltaTime;
                state = NodeState.Running;
                return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }
}
