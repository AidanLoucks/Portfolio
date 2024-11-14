using System.Collections;
using UnityEngine;

public class PlayerPreAttackState : BaseState
{
    private float chargeTimer = 0;
    private float succesfulChargeTime;

    private float hspd;
    private float vspd;

    private Vector2 directionVec;

    private BaseState _prevState;
    private HitboxSpawner _hbSpwaner;
    private InputManage _input;

    public PlayerPreAttackState(BaseState prevState, Player player, Vector2 speedVec, HitboxSpawner hbSpawner, float chargeTime)
    {
        _prevState = prevState; // The state the player was in before this ones
        hspd = speedVec.x; // Horizontal speed
        vspd = speedVec.y; // Vertical speed
        _hbSpwaner = hbSpawner;
        succesfulChargeTime = chargeTime;
        State = PlayerState.PreAttack;
        staminaCost = player.NormalStaminaCost;
    }

    /// <summary>
    /// Called once when the state is first entered (think start method)
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void EnterState(PlayerStateMachine player)
    {
        _input = player.Input;

        Player script = player.GetComponent<Player>();
        script.StopInvincibility(); // If the player is in Iframes, stop them when they attack

        player.Animator.ResetTrigger("Charged");
        player.Animator.SetTrigger("PreAttack");


        // Check the direction the mouse is in and turn the player
        directionVec = _input.MouseOctantDirection;
        player.Animator.SetFloat("Horizontal", directionVec.x);
        player.Animator.SetFloat("Vertical", directionVec.y);
    }

    /// <summary>
    /// Checks if State Machine needs to switch to new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void UpdateState(PlayerStateMachine player)
    {
        // When the player enters PreAttack, the attack button is held down.
        // If it is released before the charge time is reached, they will do a normal attack
        if (!player.Input.attack)
        {
            // If the player didnt hold for long enough or they dont have enough stamina, do a normal attack
            if (chargeTimer < succesfulChargeTime ||  player.Player.Stamina < player.Player.ChargedStaminaCost)
            {
                player.ChangeState(new PlayerAttackState(_prevState, _hbSpwaner, directionVec, player.Player.NormalDamage, player.Player.NormalStaminaCost));
            }
            else
            {
                // For some reason, Charged trigger is not being reset
                player.Animator.ResetTrigger("Charged");
                player.ChangeState(new PlayerChargedAttackState(_prevState, _hbSpwaner, player.Player.ChargedDamage, player.Player.ChargedStaminaCost));
            }
        }

        // Start the flashing to indicate the player charged attack is ready
        if(chargeTimer >= succesfulChargeTime && player.Player.Stamina >= player.Player.ChargedStaminaCost)
        {
            player.Animator.SetTrigger("Charged");
            player.Animator.ResetTrigger("PreAttack");
        }

        chargeTimer += Time.deltaTime;
    }

    /// <summary>
    /// Called every physics update. Checks for movement input and moves the player accordingly
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void FixedUpdateState(PlayerStateMachine player) 
    {
        // Slows the players vertical and horizontal speed to an icy stop (because you slide)
        LerpToZero(25);

        // Moves the player
        player.transform.position = new Vector3(player.transform.position.x + hspd * Time.fixedDeltaTime, // X
                                                    player.transform.position.y + vspd * Time.fixedDeltaTime, // Y
                                                        player.transform.position.z); // Z

        // Only if we came from the roll state
        if (_prevState.State == PlayerState.Roll)
        {
            // These allow the player to change the way they are facing,
            // opposite to the direction they are moving while charging an attack
            if (hspd != 0)
            {
                if (-directionVec.x == _input.MouseOctantDirection.x)
                {
                    directionVec.x = -directionVec.x;
                    player.Animator.SetFloat("Horizontal", -hspd);
                }
            }

            if (vspd != 0)
            {
                if (-directionVec.y == _input.MouseOctantDirection.y)
                {
                    directionVec.y = -directionVec.y;
                    player.Animator.SetFloat("Vertical", -vspd);
                }
            }
        }
        
    }

    /// <summary>
    /// Called once before transitioning to a new state
    /// </summary>
    /// <param name="player">The player that is being affected by this state machine</param>
    public override void ExitState(PlayerStateMachine player) { player.Animator.ResetTrigger("PreAttack"); }

    /// <summary>
    /// Lerps the player velocity to 0
    /// </summary>
    /// <param name="lerpVal">how fast to lerp</param>
    /// <param name="speedVec"> X - Horizontal Speed
    ///                          Y - Vertical Speed </param>
    protected void LerpToZero(int lerpVal)
    {
        //lerp velocity to 0
        if (hspd != 0)
        {
            hspd = hspd < 0 ? hspd + lerpVal * Time.deltaTime : hspd - lerpVal * Time.deltaTime;
        }
        if (vspd != 0)
        {
            vspd = vspd < 0 ? vspd + lerpVal * Time.deltaTime : vspd - lerpVal * Time.deltaTime;
        }
        if (Mathf.Abs(hspd) <= .1f)
        {
            hspd = 0;
        }
        if (Mathf.Abs(vspd) <= .1f)
        {
            vspd = 0;
        }
    }
}
