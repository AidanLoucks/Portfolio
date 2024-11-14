using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviorTree
{
    public class BossBT : Tree
    {
        public float waitTime;
        private BossSwitch switchNode;

        private Transform[] waypoints;

        private BloxorRoom _currentRoom;

        public BloxorRoom SetCurrentRoom
        {
            set
            {
                if (_currentRoom == null)
                    _currentRoom = value;
            }
        }

        public Transform[] SetWaypoints
        {
            set
            {
                if (waypoints == null)
                    waypoints = value;
            }
        }

        public void TriggerNextStage() { switchNode.NextStage(); }

        protected override Node SetUpTree()
        {
            Node root = switchNode = new BossSwitch(new List<Node>
            {
                new Timer (
                    new DeferredSequence(new List<Node>
                    {
                        new BossPatrol(transform, waypoints),
                        new BossSpawnEnemies(_currentRoom),
                    })
                    , waitTime),

                new Timer( new BossRam(transform), waitTime)
            });

            return root;
        }
    }
}


