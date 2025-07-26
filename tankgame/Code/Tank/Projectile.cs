using static Sandbox.Component;

public sealed class Projectile : Component, ITriggerListener
{
	[Property] public float Speed { get; set; } = 1800f;
	[Property] public float LifeTime { get; set; } = 2f;
	//[Property] public GameObject HitParticlePrefab { get; set; }
	[Property] public GameObject ignoreObject { get; set; }
	private Vector3 direction;

	protected override void OnStart()
	{
		direction = GameObject.WorldRotation.Forward;
	}

	protected override void OnUpdate()
	{
		// Move projectile forward
		GameObject.WorldPosition += direction * Speed * Time.Delta;

		// Reduce lifetime
		LifeTime -= Time.Delta;
		if ( LifeTime <= 0f )
		{
			GameObject.Parent.Destroy();
		}
	}
}
