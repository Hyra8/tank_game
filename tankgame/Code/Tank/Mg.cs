public sealed class Machinegun : Component
{
	[Property][Group( "Object" )] public GameObject Mgaimpoint { get; set; }
	[Property][Group( "Object" )] public PrefabScene ProjectilePrefab { get; set; }
	[Property][Group( "Object" )] public int MgAmmo { get; set; }
	
	[Property][Group( "SoundEvent" )] public SoundEvent shoot { get; set; }
	[Property][Group( "SoundEvent" )] public SoundEvent reload { get; set; }
	[Property] public float LifeTime { get; set; } = 2f;
	private float gunspreadangle = 1f;
	public int mground;
	public float remainingTime;
	private Rotation bulletdirection;
	private TimeSince fireTimer;
	private float fireInterval = 60f / 600f;
	private bool isshot = false;
	private int currentAmmo;
	protected override void OnStart()
	{
		remainingTime = 0f;
		currentAmmo = mground = 60;
	}
	protected override void OnUpdate()
	{
		bulletdirection = Mgaimpoint.WorldRotation * Rotation.FromPitch( -90f );
		if ( Input.Down( "Jump" ) && fireTimer >= fireInterval )//&& mground > 0
		{
			//Creating projectitle
			FireProjectile();
			mground --;

			//Sound
			GameObject.PlaySound( shoot );
			isshot = true;
			fireTimer = 0;
		}
	}

	private void FireProjectile()
	{

		if ( ProjectilePrefab == null )
		{
			Log.Info( "Projectile invalid!" );
			return;
		}
		var startpos = Mgaimpoint.WorldPosition;
		float randomYaw = Game.Random.Float(  -gunspreadangle, gunspreadangle );
		float randomPitch = Game.Random.Float(-gunspreadangle, gunspreadangle);
		var spreadRotation = bulletdirection * Rotation.FromYaw( randomYaw ) * Rotation.FromPitch( randomPitch );
		ProjectilePrefab.Clone( startpos, spreadRotation );
	}
}
