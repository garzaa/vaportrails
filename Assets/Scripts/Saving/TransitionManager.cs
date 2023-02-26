using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TransitionManager : SavedObject {
	public Transition transition;

	protected override void LoadFromProperties() {}

	public void LoadLastSavedScene() {
		SceneManager.LoadScene(Get<string>("scene"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["scene"] = SceneManager.GetActiveScene().name;
	}

	protected override void Initialize() {
		if (transition.subway) {
			// find the subway where the previous scene corresponds to the current scene's name
			Subway[] subways = GameObject.FindObjectsOfType<Subway>();
			foreach (Subway subway in subways) {
				if (subway.prevStop == null || subway.prevStop.ScenePath == transition.subway.previousScenePath) {
					subway.LoadRidingPlayer(transition.subway);
					break;
				}
			}
		} else if (transition.position) {
			PlayerInput.GetPlayerOneInput().gameObject.transform.position = transition.position.vec2;
		}
	}

	public void SubwayTransition(Transition.SubwayTransition subwayTransition) {
		transition.Clear();
		transition.subway = subwayTransition;
		SceneManager.LoadScene(subwayTransition.scene);
	}

	public void BeaconTransition() {}

	public void PositionTransition() {}
}
