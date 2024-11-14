using UnityEngine;

public class PlayerIdleState : BaseState
{
    private Vector2 _direction;

    public PlayerIdleState(Vector2 direction)
    {
        _direction = direction;
        State = PlayerState.Idle;
    }

    /// <summary>
    /// Called once when the state is first entered (think start method)
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void EnterState(PlayerStateMachine player)
    {
        // Set the direction the idle animation should face
        //player.Animator.SetFloat("Horizontal", _direction.x);
        //player.Animator.SetFloat("Vertical", _direction.y);

        // Change the animation state to idle
        player.Animator.SetTrigger("Idle");
    }

    /// <summary>
    /// Checks if State Machine needs to switch to new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void UpdateState(PlayerStateMachine player)
    {
        InputManage input = player.Input;

        // If directional movement keys are pressed, transition to run state
        if (input.down || input.up || input.right || input.left)
        {
            player.ChangeState(new PlayerRunState(player.Player.MoveSpeed));
        }

        // if (input.roll)
        // {
        //     player.ChangeState(new PlayerRollState(this, _direction));
        // }

        if (input.tongue)
        {
            player.ChangeState(new PlayerTongueState(player, this, player.HbSpawner, player.Player.TongueStaminaCost));
        }

        if (input.attack)
        {
            player.ChangeState(new PlayerPreAttackState(this, player.GetComponent<Player>(), _direction, player.HbSpawner, player.Player.ChargeSpeed));
        }

        // if (player.Input.spell)
        // {
        //     player.ChangeState(new PlayerSpellState(player.ActiveSpell));
        // }

    }

    public override void FixedUpdateState(PlayerStateMachine player) { }

    /// <summary>
    /// Called once before transitioning to a new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void ExitState(PlayerStateMachine player)
    {
        // Leave the Idle animation state
        player.Animator.ResetTrigger("Idle");
    }
}
