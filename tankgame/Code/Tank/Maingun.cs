public sealed class Maingun : Component
{
	[Property][Group( "Object" )] public CameraMovement CameraMovement { get; set; }
	[Property][Group( "Object" )] public GameObject GunBarrel { get; set; }
	[Property][Group( "Object" )] public SkinnedModelRenderer ModelRenderer { get; set; }
	[Property][Group( "Object" )] public ParticleConeEmitter BarrelSmoke { get; set; }
	[Property][Group( "Object" )] public ParticleConeEmitter MuzzleFlash { get; set; }
	[Property][Group( "Properties" )] public PrefabScene ProjectilePrefab { get; set; }
	[Property][Group( "Properties" )] public int MaingunAmmo { get; set; }
	[Property][Group( "Properties" )] public int CoaxialgunAmmo { get; set; }
	
	[Property][Group( "Properties" )] public float FireCooldown { get; set; } = 70f;
	[Property][Group( "SoundEvent" )] public SoundEvent shoot { get; set; }
	[Property][Group( "SoundEvent" )] public SoundEvent reload { get; set; }
	public int coaxialround;
	public int mground;
	private float lastFireTime = 0f;
	public float remainingTime;
	private Rotation bulletdirection;
	private TimeSince delaytimer;
	[Property] public float LifeTime { get; set; } = 2f;
	private bool isloop = false;
	private bool isshot = false;
	protected override void OnStart()
	{
		remainingTime = 0f;
		ModelRenderer.Sequence.PlaybackRate = 0.7f;
		coaxialround = 250;
		mground = 60;
		GameObject.PlaySound( reload );
	}
	protected override void OnUpdate()
	{
		bulletdirection = GunBarrel.WorldRotation * Rotation.FromPitch( -90f );
		if ( Input.Pressed( "Attack1" ) && (remainingTime <= 0) && MaingunAmmo > 0 )
		{
			//Creating projectitle
			FireProjectile();
			MaingunAmmo -= 1;
			lastFireTime = Time.Now;

			//Animation handeling
			ModelRenderer.Sequence.Looping = true;
			isloop = true;
			delaytimer = 0;

			//Sound
			GameObject.PlaySound( shoot );
			isshot = true;
		}
		if ( isloop && delaytimer > 0.5f )
		{
			ModelRenderer.Sequence.Looping = false;
		}
		if ( isshot && delaytimer > 0.5 )
		{
			GameObject.PlaySound( reload );
			MuzzleFlash.Rate = 0;
		BarrelSmoke.Rate = 0;
			isshot = false;
		}
		if ( isshot && delaytimer > 0.5 )
		{
			
			GameObject.PlaySound( reload );
			
			isshot = false;
		}
		
		remainingTime = FireCooldown - (Time.Now - lastFireTime);
	}

	private void FireProjectile()
	{

		if ( ProjectilePrefab == null )
		{
			Log.Info( "Projectile invalid!" );
			return;
		}
		var startpos = GunBarrel.WorldPosition;
		var gunfoward = CameraMovement.Gun.WorldRotation.Forward;
		ProjectilePrefab.Clone( startpos, bulletdirection );
		MuzzleFlash.Rate = 100;
		BarrelSmoke.Rate = 100;
		Log.Info( "Projectile Fired!" );
	}
}
