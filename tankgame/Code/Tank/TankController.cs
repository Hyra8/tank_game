using System;
using System.Threading.Tasks;

public sealed class TankController : Component
{
	//Properties
	[Property][Group( "Properties" )][Range( 0f, 2000f, 100f )] public float MaxHealth { get; set; } = 2000f;
	//Movement
	[Property][Group( "Movement" )] public float MoveSpeed { get; set; } = 600f;
	[Property][Group( "Movement" )] public float TurnSpeed { get; set; } = 0.5f;
	[Property][Group( "Movement" )] public float GroundCheckDistance { get; set; } = 50f;
	[Property][Group( "Object" )] ParticleBoxEmitter SmokeExhaustEmiter { get; set; }
	[Property][Group( "SoundEvent" )] public SoundEvent egstart { get; set; }
	[Property][Group( "SoundEvent" )] public SoundEvent egstop { get; set; }
	[Property][Group( "SoundEvent" )] public SoundEvent egloop { get; set; }


	private Rigidbody _rigidbody;
	private PhysicsBody _physicsBody;
	private bool isEngineRunning = false;
	public bool isEngineStarted = false;
	private float _health;
	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			UpdateHealth( value );
		}
	}
	protected override void OnStart()
	{
		_rigidbody = Components.Get<Rigidbody>();
		_physicsBody = _rigidbody.PhysicsBody;

		if ( _physicsBody == null )
		{
			return;
		}

		_health = MaxHealth;
	}
	protected override void OnFixedUpdate()
	{
		// Get player input
		float forwardInput = Input.AnalogMove.x; // W/S
		float turnInput = Input.AnalogMove.y;    // A/D

	}

	protected override void OnUpdate()
	{
		if ( _physicsBody == null ) return;

		float moveInput = Input.AnalogMove.x; // W/S
		float turnInput = Input.AnalogMove.y; // A/D
		Vector3 extraGravity = Vector3.Down * 3000f;
		_physicsBody.ApplyForce( extraGravity * _physicsBody.Mass );
		if ( isEngineStarted )
		{

			Vector3 forward = GameObject.LocalRotation.Forward;
			_physicsBody.Velocity = forward * moveInput * MoveSpeed;

			var currentAngular = _physicsBody.AngularVelocity;
			_physicsBody.AngularVelocity = new Vector3( 0, 0, turnInput * TurnSpeed.DegreeToRadian() );
		}
		if ( Input.Pressed( "Use" ) )
		{
			if ( isEngineStarted )
			{
				StopEngineSounds();
				isEngineStarted = false;
			}
			else
			{
				isEngineStarted = true;
				PlayEngineSounds();
			}

		}
		if ( !isEngineStarted )
		{
			SmokeExhaustEmiter.Rate = 0;
		}
		else {
			SmokeExhaustEmiter.Rate = 60;
		}
	}
	private void UpdateHealth( float newhealth )
	{
		var difference = newhealth - _health;
		_health = float.Clamp( difference, 0.0f, MaxHealth );
		if ( Health <= 0f )
		{

		}
	}

	private async void PlayEngineSounds()
	{
		GameObject.PlaySound( egstart );
		await Task.Delay( 500 );
		isEngineRunning = true;
		GameObject.PlaySound( egloop );
		_ = LoopEngineSound();
	}
	private async Task LoopEngineSound()
	{
		while ( isEngineRunning )
		{
			GameObject.PlaySound( egloop );
			await Task.Delay( 2983 );
		}
	}

	private void StopEngineSounds()
	{
		isEngineRunning = false;
		GameObject.PlaySound( egstop );
	}

	private bool IsGrounded()
	{
		Vector3 origin = Transform.World.Position;
		Vector3 direction = Vector3.Down;

		var trace = Scene.Trace.Ray( origin, origin + direction * GroundCheckDistance )
			.WithoutTags( "tank" )
			.Run();

		return trace.Hit;
	}
}
