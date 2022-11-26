using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TransitionManager : SavedObject {
	public Transition transition;

	// TODO: clear the transition at some point?
	// maybe in loadscene/loadsubway?
	// loadscene(beacon)
	// takesubway(subwaytransition)
	// then call subway arrive on load

	protected override void LoadFromProperties() {}

	public void LoadLastSavedScene() {
		SceneManager.LoadScene(Get<string>("scene"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["scene"] = SceneManager.GetActiveScene().name;
	}

	protected override void Initialize() {
		if (transition.subway) {
			Subway subway = GameObject.FindObjectOfType<Subway>();
			subway.LoadRidingPlayer(transition.subway);
		} else if (transition.position) {
			PlayerInput.GetPlayerOneInput().gameObject.transform.position = transition.position.vec2;
		}
	}

	public void SubwayTransition(Transition.SubwayTransition subwayTransition) {
		transition.Clear();
		transition.subway = subwayTransition;
	}

	public void BeaconTransition() {}

	public void PositionTransition() {}
}
