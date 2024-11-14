using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree 
{
    public class RangedEnemyBT : Tree
    {
        private float longRange = 7.5f;
        private float shortRange = 5f;

        public float moveSpeed = 2f;
        public float shootCooldown = 2f;

        private RangedEnemy rangedEnemy;
        private Rigidbody2D rb;
        private Animator anim;

        protected override void Start()
        {
            rangedEnemy = GetComponent<RangedEnemy>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            
            base.Start();
        }

        protected override Node SetUpTree()
        {
            Node root = new FailSwitch(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new CheckPlayerOutOfRange(transform, shortRange),
                    new EnemyAttack(transform, rb, anim, moveSpeed)
                }),
                new Sequence(new List<Node>
                {
                    new CheckPlayerInRange(transform, longRange),
                    new Timer(new EnemyShoot(transform, rangedEnemy, anim), shootCooldown)
                }),
            }, transform, anim, GetComponent<Enemy>().hurtbox);
            return root;
        }

        /// <summary>
        /// Draw enemy ranges
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, shortRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, longRange);
        }
    }
}





