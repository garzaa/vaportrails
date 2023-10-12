using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmokeBeastAI : AIEngine {
	override protected void InitializeEngine() {

		AIRoutine jumpAtPlayer = new AIRoutine()
			.Jump()
			.MoveToPlayerX()
			.FinishOnXDistance(3);

		Add(State.NEUTRAL, new AIRoutine()
			.MoveToPlayerX()
			.FinishOnXDistance(2f)
			.Then().Kick()
		);
		Add(State.NEUTRAL, new AIRoutine()
			.MoveToPlayerX()
			.FinishOnXDistance(5f)
			.Then().Punch()
		);

		Add(State.NEUTRAL, jumpAtPlayer.Then().Punch());
		Add(State.NEUTRAL, jumpAtPlayer.Then().Punch());
		Add(State.NEUTRAL, jumpAtPlayer.Then().Kick());
		Add(State.NEUTRAL, jumpAtPlayer.Then().Kick());
		Add(State.NEUTRAL, jumpAtPlayer.Then().Kick());
	}
}

