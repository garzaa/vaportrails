using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCheck : MonoBehaviour {

	#pragma warning disable 0649
	Collider2D targetCollider;
	public float groundGap = 1f/64f;
	#pragma warning restore 0649
	const bool drawDebug = true;

	public WallCheckData wallData = new WallCheckData();

	int layerMask = Layers.GroundMask;
	const float extendDistance = 1f/64f;
	bool touchingWallLastFrame = false;

	// avoid physics jank
    const float minHitInterval = 0.3f;
	float lastHitTime = -1000f;

	const float angleTolerance = 5f;

	void Start() {		
		targetCollider = GetComponent<Collider2D>();
	}

	bool CheckAndStoreWall(RaycastHit2D hit) {
		bool isWall = (
			hit.collider != null
			&& (Mathf.Abs(90 - Vector2.Angle(Vector2.up, hit.normal)) < angleTolerance)
		);

		if (isWall) wallData.collider = hit.collider;

		return isWall;
	}

	void Update() {
		RefreshWallData(wallData);

		Vector2 startPoint = (Vector2) targetCollider.transform.position + targetCollider.offset;
		Vector2 actualSize = new Vector2(targetCollider.bounds.size.x, targetCollider.bounds.size.y-(2*groundGap));

		float distance = targetCollider.bounds.size.x/2f + extendDistance;
		bool touchingwallThisFrame = false;

		Vector2 topStart = startPoint+(Vector2.up*actualSize.y*0.5f);
		Vector2 bottomStart = startPoint+(Vector2.down*actualSize.y*0.5f);

		//cast left
		RaycastHit2D topHit = Physics2D.Raycast(
			origin: topStart,
			direction: Vector2.left,
			distance: distance,
			layerMask: layerMask
		);
		RaycastHit2D midHit = Physics2D.Raycast(
			origin: startPoint,
			direction: Vector2.left,
			distance: distance,
			layerMask: layerMask
		);
		RaycastHit2D bottomHit = Physics2D.Raycast(
			origin: bottomStart,
			direction: Vector2.left,
			distance: distance,
			layerMask: layerMask
		);
		Debug.DrawLine(topStart, topStart+(Vector2.left*distance), Color.cyan);
		Debug.DrawLine(bottomStart, bottomStart+(Vector2.left*distance), Color.cyan);
		if (CheckAndStoreWall(topHit) || CheckAndStoreWall(bottomHit) || CheckAndStoreWall(midHit)) {
			wallData.direction = -1;
			touchingwallThisFrame = true;
		}

		// cast right
		topHit = Physics2D.Raycast(
			origin: startPoint+(Vector2.up*actualSize.y*0.5f),
			direction: Vector2.right,
			distance: distance,
			layerMask: layerMask
		);
		midHit = Physics2D.Raycast(
			origin: startPoint,
			direction: Vector2.right,
		distance: distance,
			layerMask: layerMask
		);
		bottomHit = Physics2D.Raycast(
			origin: startPoint+(Vector2.down*actualSize.y*0.5f),
			direction: Vector2.right,
			distance: distance,
			layerMask: layerMask
		);
		if (CheckAndStoreWall(topHit) || CheckAndStoreWall(bottomHit) || CheckAndStoreWall(midHit)) {
			wallData.direction = 1;
			touchingwallThisFrame = true;
		}

		if (!touchingWallLastFrame && touchingwallThisFrame) {
			if (Time.time-lastHitTime > minHitInterval) {
				wallData.hitWall = true;
				lastHitTime = Time.time;
			}
		}

		if (touchingWallLastFrame && !touchingwallThisFrame) {
			wallData.leftWall = true;
		}

		wallData.touchingWall = touchingwallThisFrame;
		touchingWallLastFrame = touchingwallThisFrame;
	}

	void RefreshWallData(WallCheckData data) {
		data.direction = 0;
		data.touchingWall = false;
		data.hitWall = false;
		data.leftWall = false;
		data.collider = null;
	}
}

[System.Serializable]
public class WallCheckData {
	public int direction;
	public bool touchingWall;
	public bool hitWall;
	public bool leftWall;
	public Collider2D collider;
}
