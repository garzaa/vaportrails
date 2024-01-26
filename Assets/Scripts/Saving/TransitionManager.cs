using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TransitionManager : SavedObject {
	public Transition transition;
	public GameObject hardLockCamera;
	public bool skipIntroFadeThisScene = false;

	Animator animator;
	float targetVolume = 1f;
	float originalVolume = 1f;
	const float FADE_TIME = 1f;
	float elapsedTime;
	float transitionEndTime;

	SaveManager saveManager;
	SpeedrunTimer speedrunTimer;

	protected override void LoadFromProperties() {}

	public void LoadLastSavedScene() {
		transition.Clear();
		SceneManager.LoadScene(Get<string>("scene"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["scene"] = SceneManager.GetActiveScene().name;
	}

	protected override void Initialize() {
		saveManager = FindObjectOfType<SaveManager>();
		speedrunTimer = FindObjectOfType<SpeedrunTimer>();
		hardLockCamera.SetActive(true);
		AudioListener.volume = 0;
		FadeAudio(1);
		animator = GetComponent<Animator>();

		if (skipIntroFadeThisScene) {
			animator.Play("ScreenUnfaded");
		} else {
			animator.Play("ScreenUnfade");
		}

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
		} else if (transition.beacon) {
			// find the thing that references the beacon, then move the player to whatever it is
			BeaconWrapper beaconWrapper = FindObjectsOfType<BeaconWrapper>().Where(
				x => x.GetBeacon == transition.beacon
			).First();
			Entity player = PlayerInput.GetPlayerOneInput().GetComponent<Entity>();
			player.gameObject.transform.position = beaconWrapper.GetPosition();
			StartCoroutine(FlipPlayer(player, beaconWrapper));
			beaconWrapper.OnLoad.Invoke();
		}
		
		StartCoroutine(DisableHardLock());
	}

	IEnumerator FlipPlayer(Entity player, BeaconWrapper beaconWrapper) {
		yield return new WaitForEndOfFrame();
		if (!player.facingRight && beaconWrapper.faceRight) player.Flip();
		else if (player.facingRight && !beaconWrapper.faceRight) player.Flip();
	}

	IEnumerator DisableHardLock() {
		yield return new WaitForSeconds(0.5f);
		hardLockCamera.SetActive(false);
	}

	void Update() {
		if (Time.time < transitionEndTime) {
			elapsedTime += Time.deltaTime;
			AudioListener.volume = Mathf.Lerp(originalVolume, targetVolume, elapsedTime/FADE_TIME);
		}

		if (Application.isEditor && Input.GetKeyDown(KeyCode.H)) {
			hardLockCamera.SetActive(!hardLockCamera.activeSelf);
		}
	}

	public void SubwayTransition(Transition.SubwayTransition subwayTransition) {
		transition.Clear();
		transition.subway = subwayTransition;
		StartCoroutine(LoadAsync(subwayTransition.scene));
	}

	public void DoorTransition(Door door) {
		PlayerInput.GetPlayerOneInput().transform.Find("PlayerRig").gameObject.SetActive(false);
		BeaconTransition(door.GetComponent<BeaconWrapper>().GetBeacon);
	}

	public void BeaconTransition(Beacon beacon) {
		transition.Clear();
		string pathToLoad = beacon.leftScene.ScenePath;
		if (SceneManager.GetActiveScene().path == beacon.leftScene.ScenePath) {
			pathToLoad = beacon.rightScene.ScenePath;
		}
		transition.beacon = beacon;
		StartCoroutine(LoadAsync(pathToLoad));
	}

	public void PositionTransition() {}

	public void SceneTransition(string scenePath) {
		transition.Clear();
		StartCoroutine(LoadAsync(scenePath));
	}

	public void StraightLoad(string scenePath) {
		transition.Clear();
		saveManager.TransitionPrep();
		SceneManager.LoadScene(scenePath);
	}

	public void FadeToBlack() {
		animator.Play("ScreenFade");
	}

	IEnumerator LoadAsync(string sceneName) {
		PlayerInput.GetPlayerOneInput().GetComponent<EntityController>().EnterCutscene(this.gameObject);
		speedrunTimer.OnTransitionStart();
		FadeAudio(0);
		FadeToBlack();
		yield return new WaitForSecondsRealtime(FADE_TIME);

		saveManager.TransitionPrep();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		asyncLoad.allowSceneActivation = false; 
		
        // wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone) {
			if (asyncLoad.progress >= 0.9f) {
				asyncLoad.allowSceneActivation = true;
			}

            yield return null;
        }
    }

	void FadeAudio(float targetVolume) {
		this.targetVolume = targetVolume;
		originalVolume = AudioListener.volume;
		elapsedTime = 0;
		transitionEndTime = Time.time + FADE_TIME;
	}
}
