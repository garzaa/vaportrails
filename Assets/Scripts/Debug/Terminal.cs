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
using Rewired;

public class Terminal : MonoBehaviour, IPointerDownHandler {

    public GameObject terminalContainer;
    public RectTransform textOutputContainer;
    public InputField input;
    public GameObject textOutputTemplate;

    delegate void TerminalCloseDelegate();
    TerminalCloseDelegate terminalClose;

    InputRecorder inputRecorder;
    AIPlayer aiPlayer;

    static Terminal t;
    const int maxScrollback = 800;

    Player currentPlayer;

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
                OnTerminalOpen();
            } else {
                OnTerminalClose();
            }
        }
    }

    void OnTerminalOpen() {
        SelectInput();
        ClearInput();
        foreach (PlayerInput input in GameObject.FindObjectsOfType<PlayerInput>()) {
            if (input.GetPlayer().controllers.hasKeyboard) {
                currentPlayer = input.GetPlayer();
                currentPlayer.controllers.hasKeyboard = false;
            }
        }
    }

    void OnTerminalClose() {
        if (terminalClose != null) terminalClose();
        ClearTerminalCloseListeners();
        currentPlayer.controllers.hasKeyboard = true;
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
		ParseCommand(command);
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
                if (aiPlayer || !aiPlayer.currentReplay) {
                    aiPlayer.StopReplay();
                    aiPlayer = null;
                } else {
                    Log("No currently playing replay to stop.");
                }
                return;
            }

            Replay replay;
            try {
                replay = JsonConvert.DeserializeObject<Replay>(File.ReadAllText($"{Application.dataPath}/{args[1]}.json"));
            } catch (System.IO.FileNotFoundException e) {
                Log(e.Message);
                return;
            }

            GameObject puppet;
            if (args.Length > 2) {
                puppet = GameObject.Find(args[2]);
            } else {
                Log("replay needs a target");
                return;
            }
            if (!puppet.GetComponent<PlayerInput>()) {
                Log("No input module on "+puppet.name+". Try one of:");
                Log("\n"+String.Join('\n', GameObject.FindObjectsOfType<PlayerInput>().Select(x=>x.name).ToList()));
                return;
            }
            AIPlayer ai = puppet.GetComponent<AIPlayer>();
            if (!ai) {
                ai = puppet.AddComponent<AIPlayer>();
            }
            ai.PlayReplay(replay);
            aiPlayer = ai;
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
