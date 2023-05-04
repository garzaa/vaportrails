using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

[ExecuteInEditMode]
public class CameraInterface : MonoBehaviour {
	CinemachineBrain cinemachine;

	#pragma warning disable 0649
	[SerializeField] CinemachineVirtualCamera mainCam;
	[SerializeField] CinemachineVirtualCamera worldLookCam;
	[SerializeField] GameObject editorCameraPoint;
	[SerializeField] GameObject originalPlayerTarget;
	[SerializeField] CinemachineTargetGroup targetGroupFollow;
	#pragma warning restore 0649


	void OnEnable() {
		cinemachine = GetComponent<CinemachineBrain>();
	}

	void Awake() {
		if (!Application.isPlaying && Application.isEditor) {
			SetMainTarget(editorCameraPoint);
		} else {
			ResetMainTarget();
		}
	}

	public void LookAtPoint(Transform target) {
		worldLookCam.m_Follow = target;
		worldLookCam.m_Priority = 20;
	}

	public void StopLookingAtPoint(Transform target) {
		if (worldLookCam.m_Follow != target) {
			return;
		}
		worldLookCam.m_Priority = 0;
		worldLookCam.m_Follow = null;
	}

	public void SetMainTarget(GameObject target) {
		mainCam.Follow = target.transform;
	}

	public void ResetMainTarget() {
		SetMainTarget(originalPlayerTarget);
	}

	public void AddFramingTarget(GameObject g) {
		targetGroupFollow.AddMember(g.transform, 0.5f, 0);
	}

	public void RemoveFramingTarget(GameObject g) {
		targetGroupFollow.RemoveMember(g.transform);
	}
}
