using UnityEngine;
using System.Collections.Generic;
using Rewired;

public class InputManager : MonoBehaviour {

    static Player rewiredPlayer = null;
    static InputManager im;
    static bool polling = false;

    public Vector2 ls;

	void Awake() {
        im = this;
        rewiredPlayer = ReInput.players.GetPlayer(0);
    }

    void Update() {
        ls = InputManager.LeftStick();
    }

    public static bool HasHorizontalInput() {
        return HorizontalInput() != 0;
    }

    public static float HorizontalInput() {
        return rewiredPlayer.GetAxis(Buttons.H_AXIS);
    }

    public static Vector2 UINav() {
        return new Vector2(
            rewiredPlayer.GetAxis(Buttons.UI_X),
            rewiredPlayer.GetAxis(Buttons.UI_Y)
        );
    }

    public static float VerticalInput() {
        return rewiredPlayer.GetAxis(Buttons.V_AXIS);
    }

    public static bool ButtonDown(string buttonName) {
        return !polling && rewiredPlayer.GetButtonDown(buttonName);
    }

    public static bool Button(string buttonName) {
        return !polling && rewiredPlayer.GetButton(buttonName);
    }

    public static bool ButtonUp(string buttonName) {
        return !polling && rewiredPlayer.GetButtonUp(buttonName);
    }

    public static bool GenericContinueInput() {
        return (
            ButtonDown(Buttons.JUMP) || GenericEscapeInput() || AttackInput()
        );
    }

    public static bool AttackInput() {
        return (
            ButtonDown(Buttons.PUNCH)
            || ButtonDown(Buttons.KICK)
        );
    }

    public static bool GenericEscapeInput() {
        return (
            InputManager.ButtonDown(Buttons.SPECIAL)
            || InputManager.ButtonDown(Buttons.INVENTORY)
            || InputManager.Button(Buttons.PAUSE)
            || InputManager.ButtonDown(Buttons.UI_CANCEL)
        );
    }

    public static Vector2 RightStick() {
        return new Vector2(
            rewiredPlayer.GetAxis("Right-Horizontal"),
            rewiredPlayer.GetAxis("Right-Vertical")
        );
    }

    public static Vector2 LeftStick() {
        return new Vector2(
            rewiredPlayer.GetAxis(Buttons.H_AXIS),
            rewiredPlayer.GetAxis(Buttons.V_AXIS)
        );
    }

    public static bool TauntInput() {
        return false;
    }

    public static Vector2 MoveVector() {
        return new Vector2(HorizontalInput(), VerticalInput());
    }

    public void OnButtonPollStart() {
        InputManager.polling = true;    
    }

    public void OnButtonPollEnd() {
        InputManager.polling = false;
    }

    public static float GetInputBufferDuration() {
		return 2f/10f;
        // return SaveManager.save.options.inputBuffer * (1f/16f);
    }
}

public static class Buttons {
    public static readonly string H_AXIS = "Horizontal";
    public static readonly string V_AXIS = "Vertical";
    public static readonly string JUMP   = "Jump";
    public static readonly string ATTACK = "Attack";
    public static readonly string PUNCH  = "Punch";
    public static readonly string KICK   = "Kick";
    public static readonly string INVENTORY = "Inventory";
    public static readonly string WALK = "Walk";

    public static readonly string SPECIAL    = "Special";
    public static readonly string PROJECTILE = "Projectile";
    public static readonly string INTERACT   = "Interact";
    public static readonly string BLOCK      = "Block";
    public static readonly string SURF       = "Surf";

    public static readonly string CONFIRM = "Confirm";
    public static readonly string PAUSE   = "Start";
    public static readonly string TABLEFT = "Tab Left";
    public static readonly string TABRIGHT = "Tab Right";

    public static readonly string UI_X = "UIHorizontal";
    public static readonly string UI_Y = "UIVertical";
    public static readonly string UI_CANCEL = "UICancel";
    public static readonly string UI_SUBMIT = "UISubmit";
}
