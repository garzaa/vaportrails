using UnityEngine;
using System.Collections.Generic;
using Rewired;
using System.Linq;

public class PlayerInput : MonoBehaviour {
    Player player = null;
    static bool polling = false;

    public int playerNum = 0;
    Controller lastActiveController;

	void Awake() {
        player = ReInput.players.GetPlayer(playerNum);
    }

    void Start() {
        if (playerNum == 0) player.AddInputEventDelegate(ShowHideMouse, UpdateLoopType.Update);
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

    void ShowHideMouse(InputActionEventData actionData) {
        lastActiveController = player.controllers.GetLastActiveController();
        Cursor.visible = (lastActiveController?.type == Rewired.ControllerType.Mouse);
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

    public bool ButtonDown(int b) {
        return !polling && player.GetButtonDown(b);
    }

    public bool Button(int b) {
        return !polling && player.GetButton(b);
    }

    public bool ButtonUp(int b) {
        return !polling && player.GetButtonUp(b);
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

    public static PlayerInput GetPlayerOneInput() {
		return GameObject.FindObjectsOfType<PlayerInput>()
			.Where(x => x.playerNum == 0)
			.First();
	}
}

public static class Buttons {
    public static readonly int H_AXIS = RewiredConsts.Action.Horizontal;
    public static readonly int V_AXIS = RewiredConsts.Action.Vertical;
    public static readonly int JUMP   = RewiredConsts.Action.Jump;
    public static readonly int PUNCH  = RewiredConsts.Action.Punch;
    public static readonly int KICK   = RewiredConsts.Action.Kick;
    public static readonly int INVENTORY = RewiredConsts.Action.Inventory;

    public static readonly int SPECIAL    = RewiredConsts.Action.Special;
    public static readonly int PROJECTILE = RewiredConsts.Action.Projectile;
    public static readonly int INTERACT   = RewiredConsts.Action.Interact;
    public static readonly int PARRY      = RewiredConsts.Action.Parry;

    public static readonly int PAUSE   = RewiredConsts.Action.Start;
    public static readonly int TABLEFT = RewiredConsts.Action.TabLeft;
    public static readonly int TABRIGHT = RewiredConsts.Action.TabRight;

    public static readonly int UI_X = RewiredConsts.Action.UIHorizontal;
    public static readonly int UI_Y = RewiredConsts.Action.UIVertical;
    public static readonly int UI_CANCEL = RewiredConsts.Action.UICancel;
    public static readonly int UI_SUBMIT = RewiredConsts.Action.UISubmit;
}
