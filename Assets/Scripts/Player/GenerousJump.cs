using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenerousJump {
	class TimeV {
		public TimeV(float v) {
			this.ySpeed = v;
			this.time = Time.time;
		}

		public float ySpeed;
		public float time;
	}
	
	const float window = 0.35f;
	readonly List<TimeV> times = new List<TimeV>();

	public void StoreVelocity(Rigidbody2D rb2d) {
		times.Add(new TimeV(rb2d.velocity.y));
	}

	public float GetHighestVY(Rigidbody2D rb2d) {
		float max = rb2d.velocity.y;
		foreach (TimeV tv in times) {
			if (tv.ySpeed > max) {
				max = tv.ySpeed;
			}
		}
		return max;
	}

	public void Update() {
		for (int i=0; i<times.Count; i++) {
			if (Time.time - times[i].time > window) {
				times.RemoveAt(i);
				// removing at the index will bring the following elements down one
				// so account for that by decrementing this
				i--;
			}
		}
	}
}
