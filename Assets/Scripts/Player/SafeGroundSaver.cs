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
		// but not a full 3, just in case they're on a safe two-block platform
		bool groundCheck = CheckHit(entity.transform.position)
			&& CheckHit(entity.transform.position + Vector3.left*0.7f)
			&& CheckHit(entity.transform.position + Vector3.right*0.7f);

		// and then make sure there's no environment damage in a 1-block radius around the character
		bool envDmgCheck = EnviroDamageClearCheck(entity.transform.position, overlapPoints);
		
		if (groundCheck && envDmgCheck) {
			data.lastSafeObject = GetGameObject(entity.transform.position).gameObject;
			data.lastSafeOffset = entity.transform.position - data.lastSafeObject.transform.position;
		}
	}
	
	static bool CheckHit(Vector2 start) {
		RaycastHit2D hit = DefaultLinecast(start);
		return (hit && Vector2.Angle(hit.normal, Vector2.up) == 0 && !hit.collider.GetComponent<UnsafeGround>());
	}

	static bool EnviroDamageClearCheck(Vector2 start, Collider2D[] results) {
		int numHits = Physics2D.OverlapCircleNonAlloc(start, 1, results);
		for (int i=0; i<numHits; i++) {
			if (results[i].GetComponent<EnvironmentHitbox>() != null) {
				return false;
			}
		}
		return true;
	}

	static GameObject GetGameObject(Vector2 start) {
		return DefaultLinecast(start).collider.gameObject;
	}

	static RaycastHit2D DefaultLinecast(Vector2 start) {
		// go an entire block down in case the player is in the air
		return Physics2D.Linecast(start, start + Vector2.down * 1f, Layers.GroundMask);
	}
}
