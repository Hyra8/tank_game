using static Sandbox.Component;

public sealed class Hitmanager : Component, ITriggerListener
{
	[Category("Components")] public TargetController UnitComponent { get; set; }
	private float dmg;
	public void OnTriggerEnter( Collider other )
	{

		if ( other.GameObject == this.GameObject )
			return;
		//Sound.Play( "hit", Transform.World.Position );
		Log.Info( "Projectile Hit!" );
		Log.Info( $"Hit object: {other.GameObject}" );
		if ( other.GameObject.Name == "MainProjectile" )
		{
			dmg = 500;
		} else
		{
			dmg = 100;
		}
		GameObject.Components.TryGet<TargetController>(out var unit);
		{
			unit.Damage(dmg);
		}
	}
}
