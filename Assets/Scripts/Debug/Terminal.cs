using System.Collections;
using Cinemachine;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Newtonsoft.Json;

public class Terminal : MonoBehaviour, IPointerDownHandler {

    public GameObject terminalContainer;
    public RectTransform textOutputContainer;
    public InputField input;
    public GameObject textOutputTemplate;

    delegate void OnTerminalClose();
    OnTerminalClose terminalClose;

    InputRecorder inputRecorder;
    AIPlayer currentReplay;

    static Terminal t;
    const int maxScrollback = 800;

    void Start() {
        t = this;
        ClearConsole();
        terminalContainer.SetActive(false);
        inputRecorder = GameObject.FindObjectOfType<InputRecorder>();
    }

    void ClearTerminalCloseListeners() {
        terminalClose = null;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            terminalContainer.SetActive(!terminalContainer.activeSelf);
            if (terminalContainer.activeSelf) {
                SelectInput();
                ClearInput();
            } else {
                if (terminalClose != null) terminalClose();
                ClearTerminalCloseListeners();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        SelectInput();
    }

    void SelectInput() {
        input.Select();
        input.ActivateInputField();
    }

    public void ClearConsole() {
        foreach (RectTransform t in textOutputContainer.transform) {
            GameObject.Destroy(t.gameObject);
        }
    }

    public void OnCommandSubmit() {
        if (string.IsNullOrWhiteSpace(input.text)) {
			input.text = "";
			return;
		}
		string command = input.text.Trim();
		Log("<color='#c7cfdd'>"+command+"</color>");
		ClearInput();
		SelectInput();
		try {
            ParseCommand(command);
        } catch (System.Exception e) {
            Log(e.Message);
            throw e;
        } 
    }

    void ParseCommand(string originalCommand) {
        string[] args = originalCommand.Split(' ');
        args[0] = args[0].ToLower();

        if (args[0].Equals("pilot")) {
            if (args.Count() < 2) {
                Log("pilot needs a player name");
                return;
            }

            // get gameobjects with playerinput, match on name
            PlayerInput[] inputSources = GameObject.FindObjectsOfType<PlayerInput>();
            bool foundTarget = false;
            foreach (PlayerInput input in inputSources) {
                if (input.gameObject.name.ToLower().Equals(args[1])) {
                    foundTarget = true;
                }
            }
            if (!foundTarget) {
                Log("no target named "+args[1]+" found. try one of: \n"+string.Join('\n', inputSources.Select(x => x.name.ToLower()).ToArray()));
            } else {
                foreach (PlayerInput input in inputSources) {
                    if (input.gameObject.name.ToLower().Equals(args[1])) {
                        CameraMainTarget(input.transform);
                        // the way controls work here:
                        // every PlayerInput/Entity pair corresponds to a Player in rewired
                        // and we just swap Controller 0 between all the players
                        input.EnableHumanControl();
                    } else {
                        input.DisableHumanControl();
                    }
                }
                Log("transferring control to "+args[1]);
            }
        } else if (args[0].Equals("record")) {
            if (args.Count() < 2) {
                Log("record needs a player name to start, or stop as the second arg");
                return;
            }

            if (args[1].Equals("stop")) {
                Log("stopped recording");
                inputRecorder.StopRecording();
                return;
            }

            // get gameobjects with playerinput, match on name
            PlayerInput[] inputSources = GameObject.FindObjectsOfType<PlayerInput>();
            bool foundTarget = false;
            foreach (PlayerInput input in inputSources) {
                if (input.gameObject.name.ToLower().Equals(args[1])) {
                    foundTarget = true;
                }
            }
            if (!foundTarget) {
                Log("no target named "+args[1]+" found. try one of: \n"+string.Join('\n', inputSources.Select(x => x.name.ToLower()).ToArray()));
            } else {
                foreach (PlayerInput input in inputSources) {
                    if (input.gameObject.name.ToLower().Equals(args[1])) {
                        inputRecorder.Arm(input);
                        Log("recorder armed for "+args[1]);
                        terminalClose += inputRecorder.StartRecording;
                        return;
                    }
                }
            }
        } else if (args[0].Equals("replay")) {
            if (args[1].Equals("stop")) {
                if (currentReplay || !currentReplay.IsPlaying()) {
                    currentReplay.StopReplay();
                    currentReplay = null;
                } else {
                    Log("No currently playing replay to stop.");
                }
                return;
            }

            // load datapath/arg1.json as a replay
            Replay replay;
            try {
                replay = JsonConvert.DeserializeObject<Replay>(File.ReadAllText($"{Application.dataPath}/{args[1]}.json"));
            } catch (System.IO.FileNotFoundException e) {
                Log(e.Message);
                return;
            }
            // for the puppet input, use arg2 or just self
            GameObject puppet;
            if (args.Length > 2) {
                puppet = GameObject.Find(args[2]);
            } else {
                puppet = this.gameObject;
            }
            if (!puppet.GetComponent<PlayerInput>()) {
                Log("No input module on "+puppet.name.ToLower()+". Try one of:");
                Log("\n"+String.Join('\n', GameObject.FindObjectsOfType<PlayerInput>().Select(x=>x.name.ToLower()).ToList()));
                return;
            }
            // add puppet input if not already there
            AIPlayer ai = puppet.GetComponent<AIPlayer>();
            if (!ai) {
                ai = puppet.AddComponent<AIPlayer>();
            }
            // enable input on it
            ai.PlayReplay(replay);
            currentReplay = ai;
            // then get the AIPlayer on it (or add it if it doesn't exist)
            // and make it start playing this replay
        }
    }

    void CameraMainTarget(Transform t) {
        GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>().m_Targets[0].target = t;
    }

    void ClearInput() {
		input.text = "";
	}

    public static void Log(string text) {
        GameObject g = Instantiate(t.textOutputTemplate, t.textOutputContainer);
        g.GetComponent<Text>().text = text;
        if (t.textOutputContainer.transform.childCount > maxScrollback) {
            GameObject.Destroy(t.textOutputContainer.transform.GetChild(0));
        }
    }

    public static void Log(object o) {
        Log(o.ToString());
    }
}
