using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnergyGate : CombatNode {
	public int energyCost = 1;

	[Input(backingValue=ShowBackingValue.Never)]
	public AttackLink input;

	[Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
	public AttackLink onPass;

	[Output(backingValue=ShowBackingValue.Never, connectionType=ConnectionType.Override)]
	public AttackLink onFail;

	public override void OnNodeEnter() {
		base.OnNodeEnter();
		if ((attackGraph.combatController as ValCombatController).currentEP.Get() >= energyCost) {
			attackGraph.MoveNode(GetNode(nameof(onPass)).Connection.node as CombatNode);
		} else {
			attackGraph.MoveNode(GetNode(nameof(onFail)).Connection.node as CombatNode);
		}
	}
}
