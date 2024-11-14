using UnityEngine;

public class PlayerChargedAttackState : BaseState
{
    private BaseState _prevState;
    private BaseAttack _attack;
    private HitboxSpawner _hbSpawner;

    private GameObject _hitbox;

    private int _dmg;

    public PlayerChargedAttackState(BaseState prevState, HitboxSpawner hbSpawner, int damage, int stamCost)
    {
        _prevState = prevState;
        _hbSpawner = hbSpawner;
        _dmg = damage;
        staminaCost = stamCost; // TODO: Move this somewhere else - Should be setting base state
        State = PlayerState.ChargedAttack;
    }

    public override void EnterState(PlayerStateMachine player)
    {
        _attack = new MidoriChargedAttack(player.transform, _hbSpawner, _dmg);
        player.Animator.SetTrigger("ChargedAttack");

        Debug.Log("Charged Attack");

        if (!player.ShouldAbort)
        {
            player.Player.UseStamina(staminaCost);
            _hitbox = _attack.SpawnHitbox();
            player.GetComponent<Player>().PlaySwordClip();
        }
        else
        {
            Debug.Log("Aborted Charged Attack");
        }
    }

    public override void UpdateState(PlayerStateMachine player)
    {
        // After Charged Attack animation is done, go back to state before Charged Attack
        if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            player.ChangeState(new PlayerIdleState(new Vector2(player.Animator.GetFloat("Horizontal"), player.Animator.GetFloat("Vertical"))));
        }
    }

    public override void FixedUpdateState(PlayerStateMachine player) { }

    public override void ExitState(PlayerStateMachine player)
    {
        _hbSpawner.DestroyHitbox(_hitbox);

        // Reset Animation
        player.Animator.ResetTrigger("ChargedAttack");
    }
}
