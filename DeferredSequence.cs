using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    // could you write me a node class that acts similar to a sequence, but the next child node doesnt get evaluated until the current one returns success?
    // i.e. a deferred sequence
    public class DeferredSequence : Node
    {
        private int currentChild = 0;

        public DeferredSequence() : base() { }
        public DeferredSequence(List<Node> children) : base(children) { }

        public override NodeState Evaluate()
        {
            NodeState childState = children[currentChild].Evaluate();
            switch(childState)
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    currentChild++;
                    if (currentChild >= children.Count)
                    {
                        currentChild = 0;
                        state = NodeState.Success;
                        return state;
                    }
                    break;
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
                default:
                    break;
            }
            
            state = NodeState.Running;
            return state;
        }
    }   

}

