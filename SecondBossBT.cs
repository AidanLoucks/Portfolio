using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree 
{
    public class SecondBossBT : Tree
    {
        public float waitTime = 2f;

        private Shogun shogun;
        private BossRoom _currentRoom;

        public BossRoom SetCurrentRoom
        {
            set
            {
                if (_currentRoom == null)
                    _currentRoom = value;
            }
        }

        protected override void Start()
        {
            shogun = GetComponent<Shogun>();
            base.Start();
        }

        protected override Node SetUpTree()
        {
            Node root = new DeferredSequence(new List<Node>
            {
                new Timer(new MovetoRandomPoint(transform, _currentRoom.roomBounds), waitTime),
                new Timer(new RandomizedSelector(new List<Node>
                {
                    new BossPulseWaveAttack(shogun),
                    new BossArchimedesSpiralAttack(shogun),
                }), waitTime)
            });
            return root;
        }
    }
}



