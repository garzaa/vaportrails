using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using Newtonsoft.Json;
using System.IO;

public class GhostRecorder : InputRecorder {
	// like an input recorder, but saves game states as well

	// initialize: add a game snapshot saver to self
	// if it doesn't exist already
	// initialize it with the player and the opponent

	/*
		high level:
		1. save inputs along with game state
		2. save them as weighted options
			2a. normalize each weight to make runtime lookup faster
		3. ???
		4. profit
	*/

	/*
		TODO: Important caveats
		- normalize input as towards/away from the player
		- grab the horizontal movement axis and invert it if the opponent is to the left, very simple
		- override the SaveInput functino to see if it's the horizontal input and then invert if necessary
	*/

	// define a data structure of hash : list<FrameInput> pairings
	// this can be raw-serialized https://www.newtonsoft.com/json/help/html/serializationguide.htm
	Dictionary<int, List<FrameInput>> ghostInputs;

	GameObject self;
	GameObject enemy;

	GameSnapshotSaver snapshot = new GameSnapshotSaver();

	int hashCollisions = 0;

	public void Arm(PlayerInput self, PlayerInput enemy) {
		this.self = self.gameObject;
		this.enemy = enemy.gameObject;
		this.player = input.GetPlayer();
		snapshot.Initialize(self.gameObject, enemy.gameObject);
	}

	override protected void SaveInput(InputActionEventData e) {
		if (ReInput.mapping.GetAction(e.actionId).type.Equals(InputActionType.Axis)) {
			int axis = (int) Mathf.Sign(e.GetAxis());
			// normalize to towards<->away from opponent
			if (PlayerInput.IsHorizontal(e.actionId) && enemy.transform.position.x<self.transform.position.x) {
				axis *= -1;
			}
			actionIDAxes[e.actionId] = axis;
		} else {
			actionIDs.Add(e.actionId);
		}
	}

	protected override void SaveSnapshot() {
		FrameInput frameInput = new FrameInput(actionIDAxes, actionIDs);
		int gameState = snapshot.GetGameStateHash();

		if (!ghostInputs.ContainsKey(gameState)) {
			ghostInputs[gameState] = new List<FrameInput>();
		} else {
			hashCollisions++;
		}
		ghostInputs[gameState].Add(frameInput);
		actionIDAxes = new Dictionary<int, int>();
		actionIDs = new HashSet<int>();
	}

	protected override void SaveRecording() {
		Terminal.Log($"Ghost recording finished with {ghostInputs.Count} states; {hashCollisions} multi-input states");
		Dictionary<int, List<WeightedFrameInput>> weightedInputs = new Dictionary<int, List<WeightedFrameInput>>();

		Terminal.Log("combining frames...");
		foreach (KeyValuePair<int, List<FrameInput>> ghostInput in ghostInputs) {
			int gameState = ghostInput.Key;
			List<FrameInput> stateInputs = ghostInput.Value;

			// first pass: add everything under that game state, don't weight them yet
			// if there's no entry for that game state hash, then add it
			if (!weightedInputs.ContainsKey(gameState)) {
				weightedInputs[gameState] = new List<WeightedFrameInput>();
			}
			// then add it to the list, keep track of the duplicate count with the normalized weight (for now)
			foreach (FrameInput stateInput in stateInputs) {
				bool duplicateInput = false;
				foreach (WeightedFrameInput weightedInput in weightedInputs[gameState]) {
					if (weightedInput.frameInput.Equals(stateInput)) {
						weightedInput.normalizedWeight += 1;
						duplicateInput = true;
						break;
					}
				}
				if (!duplicateInput) {
					weightedInputs[gameState].Add(new WeightedFrameInput(stateInput));
				}
			}
		}

		Terminal.Log("done. running weighting pass...");

		// second pass: normalize the weights
		foreach (List<WeightedFrameInput> weightedInputList in weightedInputs.Values) {
			float total = weightedInputList.Select(weightedInput => weightedInput.normalizedWeight).Sum();
			foreach (WeightedFrameInput input in weightedInputList) {
				input.normalizedWeight /= total;
			}
			weightedInputList.Sort((x, y) => x.normalizedWeight.CompareTo(y.normalizedWeight));
		}

		Terminal.Log("weighting finished, saving file");

		string fileName = $"{Application.dataPath}/{input.name}_{recordingCounter}.ghostfile.json";
		File.WriteAllText(
			fileName,
			JsonConvert.SerializeObject(new Ghostfile(weightedInputs), Formatting.Indented)
		);
		Terminal.Log("Ghost saved as "+fileName);

		frameInputs.Clear();
		ghostInputs.Clear();
	}
}

[System.Serializable]
public class WeightedFrameInput {
	public float normalizedWeight = 0;
	public FrameInput frameInput;

	public WeightedFrameInput(FrameInput f) {
		this.frameInput = f;
	}
}
