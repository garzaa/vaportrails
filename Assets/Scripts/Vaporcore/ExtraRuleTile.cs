using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class ExtraRuleTile : RuleTile {
	public TileBase[] tileWith;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
		base.GetTileData(position, tilemap, ref tileData);
	}

	public override bool RuleMatch(int neighbor, TileBase other) {
		if (tileWith != null && tileWith.Length>0) {
			ExtraRuleTile otherTile = other as ExtraRuleTile;
			bool canTile = otherTile == this;
			foreach (TileBase canTileWith in tileWith) {
				canTile |= other == canTileWith;
			}

			switch (neighbor) {
				case TilingRuleOutput.Neighbor.This: return canTile;
				case TilingRuleOutput.Neighbor.NotThis: return !canTile;
			}
		}

		return base.RuleMatch(neighbor, other);
	}

	[ContextMenu("Apply Default Object")]
    void ApplyDefaultObject() {
        foreach (TilingRule rule in m_TilingRules) {
			rule.m_GameObject = m_DefaultGameObject;
		}
    }

	[ContextMenu("Apply Base Collider Type")]
    void ApplyColliderRule() {
		foreach (TilingRule rule in m_TilingRules) {
			rule.m_ColliderType = m_DefaultColliderType;
		}
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ExtraRuleTile))]
public class MyClassEditor : RuleTileEditor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
	}
}
#endif
