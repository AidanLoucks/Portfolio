using System;
using System.Collections;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    [HideInInspector]
    public bool ShouldAbort = false;
    public GameObject tongueBlock;
    public float tongueBlockDuration = .5f;
    public bool freezeIdle = false;

    [HideInInspector]
    public bool inHitstun = false;

    private InputManage _input;
    private Animator _animator;
    private HitboxSpawner _hbSpawner;
    private Player _player;

    private BaseState _currentState;

    private Skill[] _activeSpells; // 3 or 4. Pick later
    private int currentSpell = 0;

    

    #region Get Properties for Player Components
    public InputManage Input => _input;
    public Animator Animator => _animator;
    public HitboxSpawner HbSpawner => _hbSpawner;
    public Player Player => _player;
    public Skill ActiveSpell => _activeSpells[currentSpell];
    #endregion

    #region Freeze Methods
    /// <summary>
    /// Locks player to Idle state
    /// </summary>
    public void FreezeIdle()
    {
        freezeIdle = true;

        // Change the animation state to idle and leave the player in the same direction
        ChangeState(new PlayerIdleState(new Vector2(_animator.GetFloat("Horizontal"), _animator.GetFloat("Vertical"))));
    }

    /// <summary>
    ///  Returns player to normal state
    /// </summary>
    public void UnFreezeIdle() { freezeIdle = false; }
    #endregion

    /// <summary>
    /// Should be called once per player by the InputInit script at the start of the game.
    /// </summary>
    /// <param name="input"></param>
    public void SetInputManager(InputManage input) { _input = input; }

    private void Awake()
    {
        // Get Necessary Components
        _animator = GetComponent<Animator>();
        _hbSpawner = GetComponent<HitboxSpawner>();
        _player = GetComponent<Player>();
        _activeSpells = new Skill[3];
    }

    void Start()
    {
        Hurtbox playerHurtbox = _player.hurtbox;
        playerHurtbox.SubscribeToOnHit(TriggerHitstun);

        // Start the state machine in the idle state
        _currentState = new PlayerIdleState(Vector2.zero);
        _currentState.EnterState(this);
    }

    void Update()
    {
        // If the player is in dialogue, don't update the state
        if(!freezeIdle)
            _currentState.UpdateState(this);

        // _input.SetPrevInputs();

        // Change the current skill based on the input
        if (_input.swapSpell)
            currentSpell = (currentSpell + 1) % _activeSpells.Length;
    }

    void FixedUpdate()
    {
        if(!freezeIdle)
            _currentState.FixedUpdateState(this);
    }

    public void ChangeState(BaseState newState)
    {
        // If the player doesnt have enough stamina to change state, return to old state
        if (newState.StaminaCost > Player.Stamina)
            return;

        // Change state can get called before the start runs, though it shouldnt
        if(_currentState != null)
            _currentState.ExitState(this);

        _currentState = newState;
        _currentState.EnterState(this);
    }

    public void SetToIdleState()
    {
        ChangeState(new PlayerIdleState(Vector2.zero));
    }

    private void TriggerHitstun(Hitbox hb)
    {
        ChangeState(new PlayerHitstunState(0.5f));
        ShouldAbort = true;
    }

    public IEnumerator DelayTongueBlock()
    {
        yield return new WaitForSeconds(tongueBlockDuration);
        tongueBlock.SetActive(false);
    }
}
