using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GhostTile : MonoBehaviour {
	// used for ui tiles that matter because of the gameobjects they spawn
	void Start() {
		Tilemap t = GetComponentInParent<Tilemap>();
		t.SetColor(t.WorldToCell(transform.position), new Color(1, 1, 1, 0));
	}
}
