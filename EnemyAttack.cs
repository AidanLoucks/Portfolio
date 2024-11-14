using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using BehaviorTree;

public class EnemyAttack : Node
{
    private Transform transform;
    private float speed;
    private Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHurtbox hurtbox;

    public EnemyAttack(Transform t, Rigidbody2D rb2D, Animator animator, float spd)
    {
        transform = t;
        this.speed = spd;
        target = GameManager.instance.players[0].transform;
        rb = rb2D;
        anim = animator;
        hurtbox = transform.GetComponent<Enemy>().hurtbox;
    }

    public override NodeState Evaluate()
    {
        if (!transform.gameObject.GetComponent<Enemy>().InHitstun)
        {
            if (Vector2.Distance(transform.position, target.position) > 0.1f)
            {
                // Attack
                // transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
                // rb.MovePosition(target.position);
                rb.velocity = (target.position - transform.position).normalized * speed;
                anim.SetInteger("enemyState", 2); // Seek Animation
            }
        }
        else
        {
            if (!hurtbox.isArmored)
            {
                if (anim.GetInteger("enemyState") != 4)
                    anim.SetInteger("enemyState", 4); // Hitstun Animation
            }
        }

        state = NodeState.Running;
        return state;
    }
}
