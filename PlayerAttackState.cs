using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class PlayerAttackState : BaseState
{
    private BaseState _prevState;
    private BaseAttack _attack;

    private HitboxSpawner _hbSpawner;
    private GameObject _hitbox;

    private Vector2 _dirVec;

    private int _dmg;

    public PlayerAttackState(BaseState prevState, HitboxSpawner hbSpawner, Vector2 dir, int damage, int stamCost)
    {
        _prevState = prevState;
        _hbSpawner = hbSpawner;
        _dirVec = dir;
        _dmg = damage;
        staminaCost = stamCost; // TODO: Move this somewhere else - Should be setting base state
        State = PlayerState.Attack;
    }

    public override void EnterState(PlayerStateMachine player)
    {
        // spawn the hitbox
        _attack = new MidoriNormalAttack(player.transform, _hbSpawner, _dirVec, _dmg);
        player.Animator.SetTrigger("Attack");

        // Debug.Log("Normal Attack");

        if (!player.ShouldAbort)
        {
            player.Player.UseStamina(staminaCost);
            _hitbox = _attack.SpawnHitbox();
            player.GetComponent<Player>().PlaySwordClip();
        }
    }

    public override void UpdateState(PlayerStateMachine player)
    {
        if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            player.ChangeState(new PlayerIdleState(new Vector2(player.Animator.GetFloat("Horizontal"), player.Animator.GetFloat("Vertical"))));
        }
    }

    public override void FixedUpdateState(PlayerStateMachine player)
    {
        
    }

    public override void ExitState(PlayerStateMachine player)
    {
        player.Animator.ResetTrigger("Attack");
        player.ShouldAbort = false;

        // destroy the hitbox
        _hbSpawner.DestroyHitbox(_hitbox);
    }
}

