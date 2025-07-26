using System;

public sealed class CameraMovement : Component
{
	[Property][Group( "Object" )] public TankController Tank { get; set; }
	[Property][Group( "Object" )] public GameObject Body { get; set; }
	[Property][Group( "Object" )] public GameObject Turret { get; set; }
	[Property][Group( "Object" )] public GameObject Gun { get; set; }
	[Property][Group( "Object" )] public GameObject Mg { get; set; }
	[Property][Group( "Object" )] public GameObject T72 { get; set; }
	[Property][Group( "Camera" )] public CameraComponent TankCamera { get; set; }
	[Property][Group( "Camera" )] public CameraComponent GunnerCamera { get; set; }
	[Property][Group( "Camera" )] public CameraComponent DriverCamera { get; set; }
	[Property][Group( "Camera" )] public CameraComponent MgCamera { get; set; }
	[Property][Group( "Properties" )] public float Distance { get; set; } = 650f;
	[Property][Group( "Properties" )] public float MinBarrelAngle { get; set; } = -5f;
	[Property][Group( "Properties" )] public float MaxBarrelAngle { get; set; } = 13f;
	[Property][Group( "Properties" )] public float TurretRotationSpeed { get; set; } = 28f;
	[Property][Group( "Properties" )] public float GunRotationSpeed { get; set; } = 28f;
	[Property][Group( "SoundEvent" )] public SoundEvent TurretMove { get; set; }

	//Camera mode
	public byte CameraMode = 1; // 1-Tank 2-Gunner 3-Driver
	private SoundHandle turretSound;


	private float previousTurretYaw;
	private SoundHandle sound;
	private Angles eyeAngles;

	protected override void OnAwake()
	{
		eyeAngles = Turret.WorldRotation.Angles();
		eyeAngles.roll -= 90f;
		eyeAngles.yaw -= 90;
		previousTurretYaw = Turret.LocalRotation.Angles().pitch;
		SetCamera();
	}

	protected override void OnUpdate()
	{
		UpdateEyeAngles();
		UpdateTurretRotation();
		if (CameraMode == 4 )
		{
			UpdateMgPitch();
		}
		else
		{
			UpdateMgPitch();
			UpdateGunPitch();
		}
		UpdateTankCamera();
		if ( Input.Pressed( "Scope" ) )
		{
			CameraMode++;
			if ( CameraMode > 2 )
				CameraMode = 1;
			SetCamera();
			GunnerCamera.FieldOfView = 50;
		}
		if ( Input.Pressed( "View" ) )
		{
			CameraMode++;
			if ( CameraMode > 4 )
				CameraMode = 1;
			SetCamera();
		}
		if ( Input.Pressed( "ZOOM" ) && CameraMode == 2 )
		{
			if ( GunnerCamera.FieldOfView == 50 )
			{
				GunnerCamera.FieldOfView = 20;
			}
			else GunnerCamera.FieldOfView = 50;
		}
	}


	public void SetCamera()
	{
		if ( TankCamera is null || GunnerCamera is null || DriverCamera is null || MgCamera is null )
			return;

		TankCamera.Enabled = (CameraMode == 1);
		GunnerCamera.Enabled = (CameraMode == 2);
		DriverCamera.Enabled = (CameraMode == 3);
		MgCamera.Enabled = (CameraMode == 4);
	}


	private void UpdateEyeAngles()
	{
		eyeAngles.yaw -= Input.MouseDelta.x * 0.02f;
		eyeAngles.pitch += Input.MouseDelta.y * 0.02f;
	}

	private void UpdateTankCamera()
	{

		var camOrigin = Turret.WorldPosition + Vector3.Up * 100f;

		if ( CameraMode == 1 )
		{
			var camForward = eyeAngles.ToRotation().Forward;
			var trace = Scene.Trace.Ray( camOrigin, camOrigin - camForward * Distance )
				.WithoutTags( "player", "trigger" )
				.Run();

			camOrigin = trace.Hit ? trace.HitPosition + trace.Normal : trace.EndPosition;
		}

		TankCamera.WorldPosition = camOrigin;
		TankCamera.WorldRotation = eyeAngles.ToRotation();
		MgCamera.WorldRotation = eyeAngles.ToRotation();
	}

	private void UpdateTurretRotation()
	{
		// Get the desired direction from camera and apply -90� pitch fix
		Vector3 desiredDirection = eyeAngles.ToRotation().Forward;
		desiredDirection = Rotation.FromYaw( 90f ) * desiredDirection;

		// Flatten to X/Y plane
		desiredDirection = new Vector3( desiredDirection.x, desiredDirection.y, 0 ).Normal;
		if ( desiredDirection.LengthSquared.AlmostEqual( 0f ) ) return;

		// Current forward direction of turret (flattened)
		Vector3 currentForward = Turret.WorldRotation.Forward.WithZ( 0 ).Normal;

		// Get angle difference
		float angleDelta = Vector3.GetAngle( currentForward, desiredDirection );
		Vector3 axis = Vector3.Up; // Or the axis you're rotating around
		float rotationSign = MathF.Sign( Vector3.Dot( Vector3.Cross( currentForward, desiredDirection ), axis ) );

		// Apply wrap to angle delta
		float wrappedAngle = WrapAngle( angleDelta * rotationSign );

		if ( MathF.Abs( wrappedAngle ) < 0.25f ) return;

		// Clamp to max step
		float maxStep = TurretRotationSpeed * Time.Delta;
		float step = Clamp( wrappedAngle, -maxStep, maxStep );

		// Smooth the step slightly (optional low-pass smoothing)
		step *= 0.9f;

		// Rotate around turret�s local pitch axis (left)
		Rotation rotationStep = Rotation.FromAxis( Vector3.Left, step );
		Turret.LocalRotation = rotationStep * Turret.LocalRotation;
	}

	private float Clamp( float value, float min, float max )
	{
		return MathF.Max( min, MathF.Min( max, value ) );
	}


	private void UpdateGunPitch()
	{

		float targetPitch = eyeAngles.pitch;
		var gunAngles = Gun.LocalRotation.Angles();
		float maxDelta = GunRotationSpeed * Time.Delta;

		float newRoll = MoveTowardAngle( gunAngles.roll, targetPitch, maxDelta );
		newRoll = newRoll.Clamp( -MaxBarrelAngle, -MinBarrelAngle );

		gunAngles.roll = newRoll;
		Gun.LocalRotation = gunAngles.ToRotation();
	}
	private void UpdateMgPitch()
	{
		float targetPitch = eyeAngles.pitch;
		var MgAngles = Mg.LocalRotation.Angles();
		float maxDelta = GunRotationSpeed * 1.5f * Time.Delta;

		float newRoll = MoveTowardAngle( MgAngles.roll, targetPitch-180, maxDelta );
		MgAngles.roll = newRoll;
		Mg.LocalRotation = MgAngles.ToRotation();
	}

	private float MoveTowardAngle( float current, float target, float maxDelta )
	{
		float delta = WrapAngle( target - current );
		if ( MathF.Abs( delta ) <= maxDelta )
			return target;

		return WrapAngle( current + MathF.Sign( delta ) * maxDelta );
	}

	private static float WrapAngle( float angle )
	{
		angle %= 360f;
		if ( angle > 180f ) angle -= 360f;
		if ( angle < -180f ) angle += 360f;
		return angle;
	}
}
