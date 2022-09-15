using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSnapshotSaver {

	PlayerSnapshotInfo playerInfo;
	PlayerSnapshotInfo enemyInfo;

	public void Initialize(GameObject self, GameObject enemy) {
		playerInfo = self.GetComponent<PlayerSnapshotInfo>() ? self.GetComponent<PlayerSnapshotInfo>() : self.AddComponent<PlayerSnapshotInfo>();
		enemyInfo = enemy.GetComponent<PlayerSnapshotInfo>() ? enemy.GetComponent<PlayerSnapshotInfo>() : enemy.AddComponent<PlayerSnapshotInfo>();

		playerInfo.Start();
		enemyInfo.Start();
	}

	public override int GetHashCode() {
		int i = 0;
		/* 
			32 bits of information

			0:	 	whether I am attacking
			1: 		whether opponent is attacking
			2-3:	X range of opponent: close | medium | far | no threat
			4-5: 	Y range of opponent: close | medium | far | no threat
			6-7: 	projected X range in 0.5 seconds (from this and opponent motion vectors)
			8-9: 	projected Y range in 0.5 seconds
			10: 	grounded
			11: 	stunned
			12: 	hit this frame
			13: 	opponent stunned
			14: 	airjump left
			15-31: 	free space
		*/

		if (playerInfo.inAttack) i |= 1 << 0;
		if (enemyInfo.inAttack)  i |= 1 << 1;

		float xDistance = Mathf.Abs(playerInfo.rb2d.position.x - enemyInfo.transform.position.x);
		i |= DistanceToRange(xDistance) << 2;

		float yDistance = Mathf.Abs(playerInfo.rb2d.position.y - enemyInfo.transform.position.y);
		i |= DistanceToRange(yDistance) << 4;

		Vector2 projectedPlayer = ProjectPosition(playerInfo);
		Vector2 projectedEnemy = ProjectPosition(enemyInfo);

		float projXDistance = projectedPlayer.x - projectedEnemy.x;
		i |= DistanceToRange(projXDistance) << 6;

		float projYDistance = projectedPlayer.y - projectedEnemy.y;
		i |= DistanceToRange(projYDistance) << 8;

		if (playerInfo.groundData.grounded) i |= 1 << 10;

		if (playerInfo.entity.stunned) i |= 1 << 11;

		if (playerInfo.hitThisFrame) i |= 1 << 12;

		if (enemyInfo.entity.stunned) i |= 1 << 13;

		if (playerInfo.controller && playerInfo.controller.HasAirJumps()) i |= 1 << 14;

		return i;
	}

	int DistanceToRange(float distance) {
		// convert distance to close | medium | far | no threat
		if (distance < 1.5f) return 0b00;
		else if (distance < 3f) return 0b01;
		else if (distance < 5f) return 0b10;
		else return 0b11;

	}

	Vector2 ProjectPosition(PlayerSnapshotInfo info) {
		return info.rb2d.position + info.rb2d.velocity * 0.5f;
	}
}
