using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class Terminal : MonoBehaviour, IPointerDownHandler {

    public GameObject terminalContainer;
    public RectTransform textOutputContainer;
    public InputField input;
    public GameObject textOutputTemplate;

    delegate void OnTerminalClose();
    OnTerminalClose terminalClose;

    InputRecorder inputRecorder;

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
        string command = originalCommand.ToLower();
        string[] args = command.Split(' ');

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
        }
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
