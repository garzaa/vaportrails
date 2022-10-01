using UnityEngine;
using System.Collections.Generic;
using Rewired;

public class PlayerInput : MonoBehaviour {
    Player player = null;
    static bool polling = false;

    Vector2 ls;

    public int playerNum = 0;

	void Awake() {
        player = ReInput.players.GetPlayer(playerNum);
    }

    public Player GetPlayer() {
        return player;
    }

    public void EnableHumanControl() {
        player.controllers.hasKeyboard = true;
        player.controllers.AddController(ControllerType.Joystick, 0, true);
    }

    public void DisableHumanControl() {
        player.controllers.hasKeyboard = false;
        player.controllers.RemoveController(ControllerType.Joystick, 0);
    }

    void Update() {
        ls = LeftStick();
    }

    public bool HasHorizontalInput() {
        return HorizontalInput() != 0;
    }

    public float HorizontalInput() {
        return player.GetAxis(Buttons.H_AXIS);
    }

    public Vector2 UINav() {
        return new Vector2(
            player.GetAxis(Buttons.UI_X),
            player.GetAxis(Buttons.UI_Y)
        );
    }

    public float VerticalInput() {
        return player.GetAxis(Buttons.V_AXIS);
    }

    public bool ButtonDown(string buttonName) {
        return !polling && player.GetButtonDown(buttonName);
    }

    public bool Button(string buttonName) {
        return !polling && player.GetButton(buttonName);
    }

    public bool ButtonUp(string buttonName) {
        return !polling && player.GetButtonUp(buttonName);
    }

    public bool GenericContinueInput() {
        return (
            ButtonDown(Buttons.JUMP) || GenericEscapeInput() || AttackInput()
        );
    }

    public bool AttackInput() {
        return (
            ButtonDown(Buttons.PUNCH)
            || ButtonDown(Buttons.KICK)
        );
    }

    public bool GenericEscapeInput() {
        return (
            ButtonDown(Buttons.SPECIAL)
            || ButtonDown(Buttons.INVENTORY)
            || Button(Buttons.PAUSE)
            || ButtonDown(Buttons.UI_CANCEL)
        );
    }

    public Vector2 RightStick() {
        return new Vector2(
            player.GetAxis("Right-Horizontal"),
            player.GetAxis("Right-Vertical")
        );
    }

    public Vector2 LeftStick() {
        return new Vector2(
            player.GetAxis(Buttons.H_AXIS),
            player.GetAxis(Buttons.V_AXIS)
        );
    }

    public Vector2 MoveVector() {
        return new Vector2(HorizontalInput(), VerticalInput());
    }

    public void OnButtonPollStart() {
        PlayerInput.polling = true;    
    }

    public void OnButtonPollEnd() {
        PlayerInput.polling = false;
    }

    public static float GetInputBufferDuration() {
		return 2f/10f;
        // return SaveManager.save.options.inputBuffer * (1f/16f);
    }

    public static bool IsHorizontal(int actionID) {
		return actionID == RewiredConsts.Action.Horizontal;
	}

    public static bool IsAttack(int actionID) {
        return 
            actionID == RewiredConsts.Action.Punch
            || actionID == RewiredConsts.Action.Kick
            || actionID == RewiredConsts.Action.Projectile
        ;
    }
}

// TODO: use RewiredConsts with action axes
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
    public static readonly string PARRY      = "Parry";
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
