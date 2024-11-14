using UnityEngine;

public class PlayerRollState : BaseState
{
    private Transform _transform;
    private BaseState _prevState;
    private Vector2 _direction;
    private float _rollSpeed;


    public PlayerRollState(BaseState prevState, Vector2 direction, float rollSpeed, int stamCost)
    {
        _direction = direction;
        _prevState = prevState;
        _rollSpeed = rollSpeed;
        staminaCost = stamCost;
        State = PlayerState.Roll;
    }

    /// <summary>
    /// Called once when the state is first entered (think start method)
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void EnterState(PlayerStateMachine player)
    {
        _transform = player.transform;

        player.Player.UseStamina(staminaCost);

        // Trigger the roll animation in the right direction
        player.Animator.SetFloat("Horizontal", _direction.x);
        player.Animator.SetFloat("Vertical", _direction.y);
        player.Animator.SetTrigger("Roll");

        // Roll invulnerability
        player.GetComponent<Player>().hurtbox.gameObject.SetActive(false);

        // Gets the input at the start of the state to change the direction
        // These lines are needed to change direction if the player is holding down the roll button
        // TODO: Possibly add a roll cool down. This would probably negate the need for this code
        // _direction.x = Input.GetAxisRaw("Horizontal");
        // _direction.y = Input.GetAxisRaw("Vertical");
        // 
        // player.Animator.SetFloat("Horizontal", _direction.x);
        // player.Animator.SetFloat("Vertical", _direction.y);

        // Turn off collision layers between player hurtbox and enemy hitbox
        player.GetComponent<Rigidbody2D>().excludeLayers = LayerMask.GetMask("Enemy");
    }

    /// <summary>
    /// Checks if State Machine needs to switch to new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void UpdateState(PlayerStateMachine player)
    {
        // After the roll animation is finished, return to the previous state
        if (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            player.ChangeState(_prevState);
        }

        // Dash Cancel into attack
        if (player.Input.attack)
        {
            player.ChangeState(new PlayerPreAttackState(this, player.GetComponent<Player>(), _direction * _rollSpeed, player.HbSpawner, player.Player.ChargeSpeed));
        }

        // if(player.Input.spell)
        // {
        //     player.ChangeState(new PlayerSpellState(player.ActiveSpell));
        // }
    }

    /// <summary>
    /// Called every physics update. Rolls the player in the direction they are facing
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void FixedUpdateState(PlayerStateMachine player) 
    {
        // Move the player postiion at the roll speed in the direction the player is facing
        _transform.position = new Vector3(_transform.position.x + _direction.x * _rollSpeed * Time.fixedDeltaTime, // x
                                            _transform.position.y + _direction.y * _rollSpeed * Time.fixedDeltaTime, // y
                                              _transform.position.z); // z
    }

    /// <summary>
    /// Called once before transitioning to a new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void ExitState(PlayerStateMachine player)
    {
        player.Animator.ResetTrigger("Roll");

        // Undo roll invulnerability
        player.GetComponent<Player>().hurtbox.gameObject.SetActive(true);
        // Reset Collision layers between player rigidbody and enemys
        player.GetComponent<Rigidbody2D>().excludeLayers = LayerMask.GetMask("Nothing");
    }
}
