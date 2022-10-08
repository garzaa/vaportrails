using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using Newtonsoft.Json;
using System.IO;

public class GhostRecorder : InputRecorder {
	Dictionary<int, List<FrameInput>> ghostInputs = new Dictionary<int, List<FrameInput>>();

	GameObject self;
	GameObject enemy;

	GameSnapshotSaver snapshot = new GameSnapshotSaver();

	int hashCollisions = 0;

	public List<int> stretchedAttackIDs = new List<int>();

	public void Arm(PlayerInput self, PlayerInput enemy) {
		base.Arm(self);
		this.self = self.gameObject;
		this.enemy = enemy.gameObject;
		this.player = input.GetPlayer();
		snapshot.Initialize(self.gameObject, enemy.gameObject);
	}

	override protected void SaveInput(InputActionEventData e) {
		actionIDs.UnionWith(stretchedAttackIDs);

		if (ReInput.mapping.GetAction(e.actionId).type.Equals(InputActionType.Axis)) {
			int axis = (int) Mathf.Sign(e.GetAxis());
			// normalize to towards<->away from opponent
			if (PlayerInput.IsHorizontal(e.actionId) && enemy.transform.position.x<self.transform.position.x) {
				axis *= -1;
			}
			actionIDAxes[e.actionId] = axis;
		} else {
			actionIDs.Add(e.actionId);

			// attack inputs happen relatively rarely compared to how long
			// the player "wants to be in attack"
			if (ShouldStretch(e.actionId)) {
				Debug.Log("stretching attack");
				stretchedAttackIDs.Add(e.actionId);
				StartCoroutine(CutAttackPress(e.actionId));
			}
		}
	}

	IEnumerator CutAttackPress(int actionID) {
		yield return new WaitForSecondsRealtime(pollInterval * 8f);
		stretchedAttackIDs.Remove(actionID);
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

		Terminal.Log("first pass: combining frames...");
		foreach (KeyValuePair<int, List<FrameInput>> ghostInput in ghostInputs) {
			int gameState = ghostInput.Key;
			List<FrameInput> stateInputs = ghostInput.Value;

			// first pass: add everything under that game state, don't weight them yet
			if (!weightedInputs.ContainsKey(gameState)) {
				weightedInputs[gameState] = new List<WeightedFrameInput>();
			}
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

	bool ShouldStretch(int actionID) {
		return PlayerInput.IsAttack(actionID) || actionID == RewiredConsts.Action.Special;
	}
}

[System.Serializable]
public class WeightedFrameInput {
	public float normalizedWeight = 1;
	public FrameInput frameInput;

	// needed for ahead-of-time compilation helper
	public WeightedFrameInput() {}

	public WeightedFrameInput(FrameInput f) {
		this.frameInput = f;
	}
}
