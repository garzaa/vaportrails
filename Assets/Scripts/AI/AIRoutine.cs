using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AIRoutine {
	delegate void RoutineUpdate();
	RoutineUpdate routineUpdate;
	RoutineUpdate routineStart;
	public AIRoutine nextRoutine { get; private set; }

	PlayerSnapshotInfo self;
	PlayerSnapshotInfo player;
	ComputerController controller;

	List<Func<bool>> doneChecks = new();

	float startedTime;

	public void SetInfo(PlayerSnapshotInfo self, PlayerSnapshotInfo player, ComputerController controller) {
		this.self = self;
		this.player = player;
		this.controller = controller;
	}

	public void Start() {
		startedTime = Time.time;
		if (routineStart != null) routineStart();
	}

	public void Update() {
		controller.Zero();
		if (routineUpdate != null) routineUpdate();
	}

	public bool IsDone() {
		if (doneChecks.Count == 0) return true;
		foreach (var check in doneChecks) {
			if (check()) return true;
		}
		return false; 
	}

	public AIRoutine Then() {
		AIRoutine r = this;
		while (r.nextRoutine != null) {
			r = r.nextRoutine;
		}
		AIRoutine next = new AIRoutine();
		next.SetInfo(self, player, controller);
		r.nextRoutine = next;
		return this;
	}

	public AIRoutine MoveToPlayerX() {
		routineUpdate += () => {
			if (player.transform.position.x < self.transform.position.x) {
				controller.SetActionAxis(RewiredConsts.Action.Horizontal, -1);
			} else {
				controller.SetActionAxis(RewiredConsts.Action.Horizontal, 1);
			}
		};
		return this;
	}

	public AIRoutine AimAtPlayer() {
		routineUpdate += () => {
			if (player.transform.position.x < self.transform.position.x) {
				controller.SetActionAxis(RewiredConsts.Action.Horizontal, -1);
			} else {
				controller.SetActionAxis(RewiredConsts.Action.Horizontal, 1);
			}

			if (player.transform.position.y < self.transform.position.y) {
				controller.SetActionAxis(RewiredConsts.Action.Vertical, -1);
			} else {
				controller.SetActionAxis(RewiredConsts.Action.Vertical, 1);
			}
		};

		return this;
	}

	public AIRoutine FinishOnXDistance(float dist) {
		doneChecks.Add(() => {
			return Mathf.Abs(player.transform.position.x - self.transform.position.x) < dist;
		});
		return this;
	}

	public AIRoutine FinishOnYDistance(float dist) {
		doneChecks.Add(() => {
			return Mathf.Abs(player.transform.position.y - self.transform.position.y) < dist;
		});
		return this;
	}

	public AIRoutine FinishAfterTime(float timeout) {
		doneChecks.Add(() => {
			return startedTime + timeout > Time.time;
		});
		return this;
	}

	public AIRoutine FinishOnGrounded() {
		// add a lil grace period
		doneChecks.Add(() => {
			return (Time.time > startedTime+0.1f) && self.groundData.grounded;
		});
		return this;
	}

	public AIRoutine Punch() {
		routineStart += () => {
			controller.SetActionButton(RewiredConsts.Action.Punch);
		};
		return this;
	}

	public AIRoutine Kick() {
		routineStart += () => {
			controller.SetActionButton(RewiredConsts.Action.Kick);
		};
		return this;
	}

	public AIRoutine Jump() {
		routineStart += () => {
			controller.SetActionButton(RewiredConsts.Action.Jump);
		};
		return this;
	}
}
