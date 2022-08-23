using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour {
	public GameObject bullet;
	public GameObject target;
	public AudioResource fireSound;
	public GameObject fireEffect;
	public float speed;

	GameObject currentFireEffect;

	public void Fire() {
		fireSound.PlayFrom(this.gameObject);
		GameObject b = Instantiate(bullet, this.transform.position, Quaternion.identity);
		
		Vector3 direction = target.transform.position - this.transform.position;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		b.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		b.GetComponent<Rigidbody2D>().velocity = direction * speed;
		if (fireEffect && currentFireEffect==null) {
			currentFireEffect = Instantiate(fireEffect, transform.position, transform.rotation, this.transform);
		}
	}
}
