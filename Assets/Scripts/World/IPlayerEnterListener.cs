using UnityEngine;

public interface IPlayerEnterListener {
	void OnPlayerEnter(Collider2D player);

	void OnPlayerExit() {} 
}
