using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SafeGroundSaver {
	
	public class SafeGroundData {
		public GameObject lastSafeObject;
		public Vector3 lastSafeOffset;
	}

	Entity entity;
	public SafeGroundData data { get; private set; }
	Collider2D[] overlapPoints = new Collider2D[16];

	public SafeGroundSaver(Entity e) {
		this.entity = e;
		this.data = new SafeGroundData();
	}

	public void SaveIfPossible() {
		// make sure there's a free same-elevation block on either side
		bool groundCheck = DefaultLinecast(entity.transform.position)
			&& DefaultLinecast(entity.transform.position + Vector3.left)
			&& DefaultLinecast(entity.transform.position + Vector3.right);

		// and then make sure there's no environment damage in a 1-block radius around the character
		bool envDmgCheck = EnviroDamageClearCheck(entity.transform.position, overlapPoints);
		
		if (groundCheck && envDmgCheck) {
			if (entity.gameObject.name == "Val") {
				Debug.Log("ground safe check succeeded");
			}
			data.lastSafeObject = GetGameObject(entity.transform.position).gameObject;
			data.lastSafeOffset = entity.transform.position - data.lastSafeObject.transform.position;
		}
	}
	
	public static bool DefaultLinecast(Vector2 start) {
		RaycastHit2D hit = Physics2D.Linecast(start, start + Vector2.down * 0.7f, Layers.GroundMask);
		return (hit && Vector2.Angle(hit.normal, Vector2.up) == 0 && !hit.collider.GetComponent<UnsafeGround>());
	}

	public static bool EnviroDamageClearCheck(Vector2 start, Collider2D[] results) {
		int numHits = Physics2D.OverlapCircleNonAlloc(start, 1, results);
		for (int i=0; i<numHits; i++) {
			if (results[i].GetComponent<EnvironmentHitbox>() != null) {
				return false;
			}
		}
		return true;
	}

	public static GameObject GetGameObject(Vector2 start) {
		return Physics2D.Linecast(start, start + Vector2.down * 0.7f, Layers.GroundMask).collider.gameObject;
	}
}
