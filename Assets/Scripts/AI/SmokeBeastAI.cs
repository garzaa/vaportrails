using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmokeBeastAI : AIEngine {
	override protected void InitializeEngine() {
		Add(State.NEUTRAL, new AIRoutine()
			.MoveToPlayerX()
			.FinishOnXDistance(5f)
			.Then().Kick()
		);
		Add(State.NEUTRAL, new AIRoutine()
			.MoveToPlayerX()
			.FinishOnXDistance(7f)
			.Then().Punch()
		);

		Add(State.NEUTRAL, new AIRoutine()
			.MoveToPlayerX()
			.FinishOnXDistance(7f)
			.Then()
			.AimDown()
			.Punch()
		);

		Add(State.NEUTRAL, JumpAtPlayer().Then().Punch().FinishOnGrounded().Then().AimDown().Punch());
		Add(State.NEUTRAL, JumpAtPlayer().Then().Punch());
		Add(State.NEUTRAL, JumpAtPlayer().Then().Kick());
		Add(State.NEUTRAL, new AIRoutine().MoveToPlayerX().FinishOnXDistance(3).Then().AimDown().Punch());
		Add(State.NEUTRAL, new AIRoutine().MoveToPlayerX().FinishOnXDistance(3).Then().AimDown().Punch());
	}
	
	AIRoutine JumpAtPlayer() {
		return new AIRoutine()
			.Jump()
			.MoveToPlayerX()
			.FinishAfterTime(2)
			.FinishOnGrounded()
			.FinishOnXDistance(3);
	}
}

