using UnityEngine;

public abstract class BaseState
{
    protected int staminaCost = 0;
    public int StaminaCost => staminaCost;

    public PlayerState State { get; protected set; }

    public abstract void EnterState(PlayerStateMachine player);

    public abstract void UpdateState(PlayerStateMachine player);

    public abstract void FixedUpdateState(PlayerStateMachine player);

    public abstract void ExitState(PlayerStateMachine player);

}

public enum PlayerState
{
    Idle,
    Run,
    Roll,
    PreAttack,
    Attack,
    ChargedAttack,
    Tongue,
    Hit,
    Knockback,
    Dead
}