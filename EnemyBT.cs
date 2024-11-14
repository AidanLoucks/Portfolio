using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using Tree = BehaviorTree.Tree;

public class EnemyBT : Tree
{
    public float seekRange = 4f;
    public float speed = 2f;

    private Transform roomBounds;
    private Rigidbody2D rb;
    private Animator anim;

    public Transform SetRoomBounds
    {
        set
        {
            if (roomBounds == null)
                roomBounds = value;
        }
    }

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        base.Start();
    }

    protected override Node SetUpTree()
    {
        // Node root = new Selector(new List<Node>
        // {
        //     new Sequence(new List<Node>
        //     {
        //         new CheckPlayerInRange(transform, seekRange),
        //         new EnemyAttack(transform, rb, anim, speed),
        //     }),
        //     new EnemyPatrol(transform, rb, speed, roomBounds),
        // });
        // return root;

        // just have the enemy move at the players
        Node root = new EnemyAttack(transform, rb, anim, speed);
        return root;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we collide with an enemy
        if (collision.transform.CompareTag("Enemy"))
        {
            // If they are in hitstun
            if (collision.gameObject.GetComponent<Enemy>().InHitstun)
            {
                // Put this in hitstun
                GetComponent<Enemy>().InHitstun = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, seekRange);
    }
}
