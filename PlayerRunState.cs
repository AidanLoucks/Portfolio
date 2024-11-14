using System.Threading;
using UnityEngine;

public class PlayerRunState : BaseState
{
    private InputManage _input;
    private Transform _transform;

    Vector2 movementVector;
    private float hspd;
    private float vspd;
    private float _speed = 8f; // TODO: Get this from a player stats script

    private float lastHspd;
    private float lastVspd;

    public PlayerRunState(float speed)
    {
        _speed = speed;
        State = PlayerState.Run;
    }

    /// <summary>
    /// Called once when the state is first entered (think start method)
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void EnterState(PlayerStateMachine player)
    {
        _input = player.Input;
        _transform = player.transform;

        movementVector = new Vector2(hspd, vspd);

        // Change the animation state to run
        player.Animator.SetTrigger("Movement");
    }

    /// <summary>
    /// Checks if State Machine needs to switch to new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void UpdateState(PlayerStateMachine player)
    {
        // TODO: Need to stop player from rolling in place
        // Last direction player was facing
        // Vector2 movementVector = new Vector2(hspd, vspd);
        // if(movementVector == Vector2.zero && _input.LastDirectionalVector != Vector2.zero) 
        // {
        //     movementVector = _input.LastDirectionalVector;
        // }

        // If no directional movement keys are pressed, transition to idle state
        if (!_input.down && !_input.up && !_input.right && !_input.left)
        {
            
            player.ChangeState(new PlayerIdleState(movementVector.normalized));
        }

        if (_input.roll)
        {
            player.ChangeState(new PlayerRollState(this, movementVector.normalized, player.Player.RollSpeed, player.Player.RollStaminaCost));
        }

        if (_input.tongue)
        {
            player.ChangeState(new PlayerTongueState(player, this, player.HbSpawner, player.Player.TongueStaminaCost));
        }

        if (_input.attack)
        {
            player.ChangeState(new PlayerPreAttackState(this, player.GetComponent<Player>(), movementVector, player.HbSpawner, player.Player.ChargeSpeed));
        }
    }

    /// <summary>
    /// Called every physics update. Checks for movement input and moves the player accordingly
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void FixedUpdateState(PlayerStateMachine player) 
    {
        // This prioritizes vertical movement over horizontal movement
        // TODO: Prioritize the inital input direction

        #region Movent Input Checks
        if (_input.up)
        {
            vspd = _speed;

            //account for diagonal movement - important for knockback manipulation in the future
            if (_input.left || _input.right)
            {
                hspd = _input.left ? -_speed : _speed;
            }
            else
            {
                hspd = 0;
            }
        }
        else if (_input.down)
        {
            vspd = -_speed;

            //account for diagonal movement - important for knockback manipulation in the future
            if (_input.left || _input.right)
            {
                hspd = _input.left ? -_speed : _speed;
            }
            else
            {
                hspd = 0;
            }
        }
        else if (_input.left)
        {
            hspd = -_speed;

            //account for diagonal movement - important for knockback manipulation in the future
            if (_input.up || _input.down)
            {
                vspd = _input.down ? -_speed : _speed;
            }
            else
            {
                vspd = 0;
            }
        }
        else if (_input.right)
        {
            hspd = _speed;

            //account for diagonal movement - important for knockback manipulation in the future
            if (_input.up || _input.down)
            {
                vspd = _input.down ? -_speed : _speed;
            }
            else
            {
                vspd = 0;
            }

        }
        #endregion

        // Move the player
        _transform.position = new Vector3(_transform.position.x + hspd * Time.fixedDeltaTime, // x
                                            _transform.position.y + vspd * Time.fixedDeltaTime, // y
                                              _transform.position.z); // z


        // Change the animator parameters to match the movement
        movementVector = new Vector2(hspd, vspd).normalized; // This keeps the anim values locked between -1 and 1. Not needed, but cleaner
        player.Animator.SetFloat("Horizontal", movementVector.x);
        player.Animator.SetFloat("Vertical", movementVector.y);
    }

    /// <summary>
    /// Called once before transitioning to a new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void ExitState(PlayerStateMachine player)
    {
        // No longer in movement state
        player.Animator.ResetTrigger("Movement");
    }
}
