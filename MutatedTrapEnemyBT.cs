using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using Tree = BehaviorTree.Tree;

public class MutatedTrapEnemyBT : Tree
{
    public float moveSpeed = 2f;
    public float trapCooldown = 2f;
    public float throwForce = 5f;

    private Transform roomBounds;
    private GameObject player;

    protected override void Start()
    {
        roomBounds = GameManager.instance.currentRoom.roomBounds;
        player = GameManager.instance.players[0];
        base.Start();
    }

    protected override Node SetUpTree()
    {
        Node root = new Sequence(new List<Node>
        {
            new Timer(new ThrowTrap(player.transform, GetComponent<MutatedEnemy>(), throwForce), trapCooldown),
            new MutatedMoveToPoint(roomBounds, transform, player.transform, moveSpeed)
        });
        return root;
    }
}
