using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviorTree
{
    /// <summary>
    /// Will run a randm child node. 
    /// On success, the node will randomize the children again for the next evaluation.
    /// </summary>
    public class RandomizedSelector : Node
    {
        public RandomizedSelector() : base() { }
        public RandomizedSelector(List<Node> children) : base(children) { ProceduralGeneration.Shuffle(children); }

        public override NodeState Evaluate()
        {
            switch(children[0].Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    ProceduralGeneration.Shuffle(children);
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
    }
}
