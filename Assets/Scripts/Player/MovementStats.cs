using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/MovementStats")]
public class MovementStats : ScriptableObject {
	[SerializeField] float _runSpeed = 4.5f;
	[SerializeField] float _gndAcceleration = 175;
	[SerializeField] float _airAcceleration = 80;
	[SerializeField] float _jumpForce = 8f;
	[SerializeField] float _airFriction = 0.3f;
	[SerializeField] float _maxFallSpeed = -14f;
	[SerializeField] float _maxWallSlideSpeed = -4f;

	public float runSpeed => _runSpeed;
	public float gndAcceleration => _gndAcceleration;
	public float airAcceleration => _airAcceleration;
	public float jumpForce => _jumpForce;
	public float airFriction => _airFriction;
	public float maxFallSpeed => _maxFallSpeed;
	public float maxWallSlideSpeed => _maxWallSlideSpeed;
}
