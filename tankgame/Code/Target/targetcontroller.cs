using Sandbox;

public sealed class TargetController : Component
{
	
	[Property][Group( "Properties" )][Range( 0f, 500f, 10f )] public float MaxHealth { get; set; } = 500f;
	private float _health;
	public float Health
	{
		get { return _health; }
		set { UpdateHealth( value ); }
	}
	protected override void OnStart()
	{
		_health = MaxHealth;
	}
	protected override void OnUpdate()
	{

	}

	private void UpdateHealth( float newhealth ) { 
		var difference = newhealth - Health;
		_health = float.Clamp(newhealth, 0f, MaxHealth );

		if ( difference < 0f )
		{

		}
		if ( Health <= 0f ) {
			GameObject.Destroy();
		}
	}

	public void Damage( float damage ) { 
		Health -= damage;
	}
}
