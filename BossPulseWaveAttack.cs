using System.Collections;
using UnityEngine;

using BehaviorTree;

public class BossPulseWaveAttack : Node
{
    Shogun _shogun;
    float bulletSpeed = 5f;
    int _numberOfBullets = 32;
    float _waveDelay = 2f;
    int _waveCount = 3;

    bool attackStarted = false;
    bool attackFinished = false;

    public BossPulseWaveAttack(Shogun shogun)
    {
        _shogun = shogun;
    }

    public override NodeState Evaluate()
    {
        // If the attack has not started, start the attack
        if(attackStarted == false)
        {
            _shogun.StartCoroutine(SpawnPulseWave());
            attackStarted = true;
            return NodeState.Running;
        }
        // If the attack was started and has finished, return success
        else if(attackFinished)
        {
            attackStarted = false;
            attackFinished = false;
            return NodeState.Success;
        }
        else // If the attack is still running, return running
        {
            return NodeState.Running;
        }
    }

    IEnumerator SpawnPulseWave()
    {
        float angle = 0f;
        float angleIncrement = 360f / _numberOfBullets;

        for (int i = 0; i < _waveCount; i++)
        {
            for (int j = 0; j < _numberOfBullets; j++)
            {
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                _shogun.Shoot(direction, bulletSpeed);
                
                angle += angleIncrement;
            }

            angle = 0f;
            yield return new WaitForSeconds(_waveDelay);
        }

        attackFinished = true;
    }
}
