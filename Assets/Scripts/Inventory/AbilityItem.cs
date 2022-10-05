using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Data/ItemAbility")]
public class AbilityItem : Item {
	[TextArea]
	public string instructions;
}
