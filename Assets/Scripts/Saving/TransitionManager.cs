using UnityEngine;
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

	protected override void LoadFromProperties() {}

	public void LoadLastSavedScene() {
		SceneManager.LoadScene(Get<string>("scene"));
	}

	protected override void SaveToProperties(ref Dictionary<string, object> properties) {
		properties["scene"] = SceneManager.GetActiveScene().name;
	}

	protected override void Initialize() {
		hardLockCamera.SetActive(true);
		AudioListener.volume = 0;
		FadeAudio(1);
		animator = GetComponent<Animator>();

		if (!skipIntroFadeThisScene) {
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
		}
		
		transition.Clear();
		StartCoroutine(DisableHardLock());
	}

	IEnumerator DisableHardLock() {
		yield return new WaitForEndOfFrame();
		hardLockCamera.SetActive(false);
	}

	void Update() {
		if (Time.time < transitionEndTime) {
			elapsedTime += Time.deltaTime;
			AudioListener.volume = Mathf.Lerp(originalVolume, targetVolume, elapsedTime/FADE_TIME);
		}
	}

	public void SubwayTransition(Transition.SubwayTransition subwayTransition) {
		transition.subway = subwayTransition;
		SceneManager.LoadScene(subwayTransition.scene);
	}

	public void BeaconTransition() {}

	public void PositionTransition() {}

	public void SceneTransition(string scenePath) {
		StartCoroutine(LoadAsync(scenePath));
	}

	public void StraightLoad(string scenePath) {
		SceneManager.LoadScene(scenePath);
	}

	public void FadeToBlack() {
		animator.Play("ScreenFade");
	}

	IEnumerator LoadAsync(string sceneName) {
		FadeAudio(0);
		FadeToBlack();
		yield return new WaitForSeconds(FADE_TIME);

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
