using System.Collections.Generic;

namespace BehaviorTree 
{
    public class Switch : Node
    {
        private int stage = 0;

        public Switch() : base() {}
        public Switch(List<Node> children) : base(children) {}


        public void NextStage() { stage++; }

        public override NodeState Evaluate()
        {
            switch (children[stage].Evaluate())
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
    }
}


