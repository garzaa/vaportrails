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

    [TextArea]
    public string executeOnStart = "";

    delegate void TerminalCloseDelegate();
    TerminalCloseDelegate terminalClose;

    InputRecorder inputRecorder;
    GhostRecorder ghostRecorder;
    AIPlayer aiPlayer;

    static Terminal t;
    const int maxScrollback = 800;

    Player currentPlayer;

    public static bool IsOpen() {
        return t.terminalContainer.activeSelf;
    }

    void Start() {
        t = this;
        ClearConsole();
        terminalContainer.SetActive(false);
        inputRecorder = GameObject.FindObjectOfType<InputRecorder>();
        ghostRecorder = GameObject.FindObjectOfType<GhostRecorder>();
        foreach (String command in executeOnStart.Split("\n")) {
            ParseCommand(command.Trim());
        }
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
                if (input.gameObject.name.Equals(args[1])) {
                    foundTarget = true;
                }
            }
            if (!foundTarget) {
                Log("no target named "+args[1]+" found. try one of: \n"+string.Join('\n', inputSources.Select(x => x.name).ToArray()));
            } else {
                foreach (PlayerInput input in inputSources) {
                    if (input.gameObject.name.Equals(args[1])) {
                        CameraMainTarget(input.transform);
                        // the way controls work here:
                        // every PlayerInput/Entity pair corresponds to a Player in rewired
                        // and we just swap Controller 0 between all the players
                        input.EnableHumanControl();
                        currentPlayer = input.GetPlayer();
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
                inputRecorder.StopRecording();
                return;
            }

            // get gameobjects with playerinput, match on name
            PlayerInput input = GetPlayerInputByName(args[1]);
            if (input) {
                inputRecorder.Arm(input);
                Log("recorder armed for "+args[1]);
                terminalClose += inputRecorder.StartRecording;
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
            Log("Replaying "+args[1]+" on "+args[2]);
            ai.PlayReplay(replay);
            aiPlayer = ai;
        } else if (args[0].Equals("ghost")) {
            // ghost save Target Opponent? ghost save Target?
            // ghost save Target with auto-grabbed Opponent
            if (args[1].Equals("watch")) {
                if (args.Count() < 2) {
                    Log("ghost watch needs a target");
                    return;
                }
                // get gameobjects with playerinput, match on name
                PlayerInput player = GetPlayerInputByName(args[2]);
                PlayerInput opponent = null;
                foreach (PlayerInput playerInput in GameObject.FindObjectsOfType<PlayerInput>()) {
                    if (playerInput.name != player.name) {
                        opponent = playerInput;
                        break;
                    }
                }
                ghostRecorder.Arm(player, opponent);
                Log("ghost watch armed for "+args[2]);
                terminalClose += ghostRecorder.StartRecording;
            } else if (args[1] == "save") {
                ghostRecorder.StopRecording();
            } else if (args[1] == "play") {
                if (args.Count() < 4) {
                    Log("usage: ghost play [ghostfile] [target] [?opponent]");
                }
                Ghostfile ghostfile;
                try {
                    ghostfile = JsonConvert.DeserializeObject<Ghostfile>(File.ReadAllText($"{Application.dataPath}/{args[2]}.ghostfile.json"));
                } catch (System.IO.FileNotFoundException e) {
                    Log(e.Message);
                    return;
                }
                PlayerInput puppet = GetPlayerInputByName(args[3]);
                AIPlayer ai = puppet.GetComponent<AIPlayer>();
                if (!ai) ai = puppet.gameObject.AddComponent<AIPlayer>();

                PlayerInput opponent = null;
                if (args.Count() >= 5) {
                    opponent = GetPlayerInputByName(args[4]);
                } else {
                    foreach (PlayerInput input in GameObject.FindObjectsOfType<PlayerInput>()) {
                        if (input != puppet) {
                            opponent = input;
                            break;
                        }
                    }
                    if (opponent == null) {
                        Log("no player named "+args[4]+" found");
                        return;
                    }
                }
                Log($"Playing ghostfile on {puppet.gameObject.name} with opponent {opponent.gameObject.name}");
                ai.PlayGhost(ghostfile, opponent.gameObject);
                aiPlayer = ai;
            } else if (args[1] == "stop") {
                foreach (AIPlayer a in GameObject.FindObjectsOfType<AIPlayer>()) {
                    Log("stopping any ghost replays");
                    a.StopGhost();
                }
                return;
            }
        }
    }

    PlayerInput GetPlayerInputByName(string name) {
        PlayerInput[] inputSources = GameObject.FindObjectsOfType<PlayerInput>();
        bool foundTarget = false;
        foreach (PlayerInput input in inputSources) {
            if (input.gameObject.name.Equals(name)) {
                foundTarget = true;
            }
        }
        if (!foundTarget) {
            Log("no player named "+name+" found. try one of: \n"+string.Join('\n', inputSources.Select(x => x.name).ToArray()));
            return null;
        } else {
            foreach (PlayerInput input in inputSources) {
                if (input.gameObject.name.Equals(name)) {
                    return input;
                }
            }
        }
        return null;
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
