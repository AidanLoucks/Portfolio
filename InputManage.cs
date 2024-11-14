using System;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: Add ability input check for controller (? key)
// TODO: Add swap spell input check for controller (? key)

public class InputManage : MonoBehaviour
{
    //-------- FIELDS --------
    public int inputId;
    public bool isGamepad = false;
    public Gamepad gamepadId;

    public bool checkInput = true;
    public bool allowCombat = true;

    //possible device inputs
    public bool left;
    public bool right;
    public bool up;
    public bool down;
    public bool start;
    public bool attack;
    public bool tongue;
    public bool roll;
    public bool spell;
    public bool swapSpell;
    public bool ability;
    public bool interact;


    // Mouse Vector Direction from Player
    private Vector2 mouseDirection;
    private Vector2 lastDirecional;


    public Vector2 MouseOctantDirection { get { return mouseDirection; } }
    public Vector2 LastDirectionalVector => lastDirecional;

    public enum Inputs
    {
        Up,
        Down,
        Left,
        Right,
        Start,
        Attack,
        Tongue,
        Roll,
        Spell,
        SwapSpell,
        Ability
    }

    private void Awake()
    {
        GameManager.instance.InputManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!checkInput)
            return;

        //Start of all gamepad input processing
        if (isGamepad)
        {
            //if (gamepadId.aButton.IsPressed())
            //{

            //    //inputText.GetComponent<TextMeshProUGUI>().text = inputText.name + ": A pressed";
            //    Debug.Log(inputId + ": A");
            //}
            //else
            //{
            //    //inputText.GetComponent<TextMeshProUGUI>().text = inputText.name + ": A not pressed";
            //}

            #region Movement
            up = gamepadId.dpad.up.IsPressed();
            down = gamepadId.dpad.down.IsPressed();
            left = gamepadId.dpad.left.IsPressed();
            right = gamepadId.dpad.right.IsPressed();
            roll = gamepadId.buttonSouth.IsPressed() || gamepadId.rightShoulder.IsPressed() || gamepadId.leftShoulder.IsPressed();
            #endregion


            #region Combat/Other
            if (!allowCombat)
            {
                attack = gamepadId.buttonWest.IsPressed();
                tongue = gamepadId.buttonNorth.IsPressed();
                start = gamepadId.startButton.IsPressed();
                spell = gamepadId.buttonEast.IsPressed();
            }
            #endregion

            // NEED CONTROLLER SUPPORT FOR INTERACTING
            // interact = gamepadId.

            // swapSpell = gamepadId.buttonSomething.IsPressed();
            // ability = gamepadId.buttonSomething.IsPressed();

        }

        //start of all keyboard input processing
        else
        {
            //if (Keyboard.current.spaceKey.isPressed)
            //{

            //    //inputText.GetComponent<TextMeshProUGUI>().text = inputText.name + ": Space pressed";
            //    Debug.Log(inputId + ": space");
            //}
            //else
            //{
            //    //inputText.GetComponent<TextMeshProUGUI>().text = inputText.name + ": Space not pressed";
            //}

            // I added inputs for left mouse button attack and space key roll for now
            // ^^ feel free to get rid of/move this stuff to the mouse and keyboard input processing when it is made - colton

            #region Movement
            up = Keyboard.current.wKey.isPressed;
            down = Keyboard.current.sKey.isPressed;
            left = Keyboard.current.aKey.isPressed;
            right = Keyboard.current.dKey.isPressed;
            roll = Keyboard.current.oKey.isPressed || Keyboard.current.spaceKey.isPressed;
            #endregion

            #region Combat/Other
            if (allowCombat)
            {
                attack = Keyboard.current.uKey.isPressed || Mouse.current.leftButton.isPressed;
                tongue = Keyboard.current.iKey.isPressed || Mouse.current.rightButton.isPressed;
                start = Keyboard.current.escapeKey.isPressed;
                spell = Keyboard.current.pKey.isPressed || Keyboard.current.eKey.isPressed;
                swapSpell = Keyboard.current.rKey.isPressed;
                ability = Keyboard.current.qKey.isPressed;
                interact = Keyboard.current.eKey.isPressed;
            }
            #endregion

        }


        // Calculate mouse direction
        /*
        Vector2 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseDirection = new Vector2(
            mousePos.x - transform.position.x,
            mousePos.y - transform.position.y
        ).normalized;
        */

        // mouseDirection = (Input.mousePosition - transform.position).normalized;
        // 
        // // Debug.Log(mouseDirection);
        // Debug.Log(Mathf.Atan2(mouseDirection.x, mouseDirection.y) * Mathf.Rad2Deg);


        Vector3 mousePos = Input.mousePosition;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 mouseOffset = mousePos - screenCenter;

        mouseDirection = RoundVectorToNearest45Degree(new Vector2(mouseOffset.x, mouseOffset.y).normalized);

        bool anyKeyPressed = false;
        Vector2 currentInput = new Vector2(
            (right ? 1 : 0) - (left ? 1 : 0),
            (up ? 1 : 0) - (down ? 1 : 0)
        );

        if(currentInput != Vector2.zero)
        {
            lastDirecional = currentInput;
            anyKeyPressed = true;
        }

        if(!anyKeyPressed && lastDirecional == Vector2.zero)
        {
            lastDirecional = Vector2.right;
        }
    }


    /// <summary>
    /// Takes a unit vector and rounds it to the nearest 45 degree angle
    /// </summary>
    /// <param name="vector">Vector2 to round</param>
    /// <returns>1 of 8 closest unit Vectors</returns>
    public static Vector2 RoundVectorToNearest45Degree(Vector2 vector)
    {
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        float roundedAngle = Mathf.Round(angle / 45) * 45;
        float roundedAngleInRadians = roundedAngle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(roundedAngleInRadians), Mathf.Sin(roundedAngleInRadians));
    }
}
