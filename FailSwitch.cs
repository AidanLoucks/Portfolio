using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree 
{
    public class FailSwitch : Node
    {
        private int stage = 0;
        private Transform transform;
        private Animator anim;
        private EnemyHurtbox hurtbox;

        public FailSwitch() : base() { }
        public FailSwitch(List<Node> children, Transform t, Animator anim, EnemyHurtbox hb) 
            : base(children)
        { 
            transform = t; 
            this.anim = anim;
            hurtbox = hb;
        }

        public void NextStage() { stage = (stage + 1) % children.Count; }

        public override NodeState Evaluate()
        {
            if (!transform.gameObject.GetComponent<Enemy>().InHitstun)
            {
                switch (children[stage].Evaluate())
                {
                    case NodeState.Failure:
                        state = NodeState.Failure;
                        NextStage();
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
                if (!hurtbox.isArmored)
                {
                    if (anim.GetInteger("enemyState") != 4)
                        anim.SetInteger("enemyState", 4); // Hitstun Animation
                }

                return NodeState.Running;
            }
            
        }
    }
}


