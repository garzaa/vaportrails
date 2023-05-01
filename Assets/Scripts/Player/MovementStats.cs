using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/MovementStats")]
public class MovementStats : ScriptableObject {
	// this is so they can be edited without being changed at runtime
	[SerializeField] float _runSpeed = 4.5f;
	[SerializeField] float _gndAcceleration = 175;
	[SerializeField] float _airAcceleration = 80;
	[SerializeField] float _jumpSpeed = 8f;
	[SerializeField] float _airFriction = 0.3f;
	[SerializeField] float _maxFallSpeed = -14f;
	[SerializeField] float _maxWallSlideSpeed = -4f;
	[SerializeField] int _maxAirJumps = 1;
	[SerializeField] int _maxAirDashes = 1;
	[SerializeField] float _dashSpeed = 6f;
	[SerializeField] float _dashCooldown = 0.6f;
	[SerializeField] float _shortHopCutoffVelocity = 2f;
	[SerializeField] bool _dashCancel = true;

	public float runSpeed => _runSpeed;
	public float gndAcceleration => _gndAcceleration;
	public float airAcceleration => _airAcceleration;
	public float jumpSpeed => _jumpSpeed;
	public float airFriction => _airFriction;
	public float maxFallSpeed => _maxFallSpeed;
	public float maxWallSlideSpeed => _maxWallSlideSpeed;
	public int maxAirJumps => _maxAirJumps;
	public int maxAirDashes => _maxAirDashes;
	public float dashSpeed => _dashSpeed;
	public float dashCooldown => _dashCooldown;
	public float shortHopCutoffVelocity => _shortHopCutoffVelocity;
	public bool canDashCancel => _dashCancel;
}
