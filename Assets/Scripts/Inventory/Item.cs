using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Data/Item")]
public class Item : ScriptableObject {
	public Sprite icon;
	public Sprite detail;
	public bool single;
	public int cost = 0;

	[TextArea]
	public string description;
	
}
