using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationPrefabInterface : MonoBehaviour {
	public GameObject effectPoint;

	public void InstantiateEffect(GameObject effect) {
		Instantiate(effect, effectPoint.transform.position, Quaternion.identity, null);
	}
	
	public void InstantiateFollowingEffect(GameObject effect) {
		Instantiate(effect, effectPoint.transform.position, Quaternion.identity, effectPoint.transform.parent);
	}
}
