using System.Collections.Generic;
using System.Linq;

public sealed class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	[Property] public SceneFile MainMenuScene { get; set; }
	[Property] public SceneFile GameScene { get; set; }
	[Property] public SceneFile OverScene {get; set;}
	[Property] public int NumberOfTarget { get; set; }
	[Property] public List<GameObject> Targets { get; set; }
	[Property] public bool isSetting { get; set; }
	[Property] public SoundEvent Music {  get; set; }
	protected override void OnStart()
	{
		NumberOfTarget = Targets.Where( target => target.IsValid ).Count();
		GameObject.PlaySound(Music);
	}
	protected override void OnUpdate()
	{
		Log.Info( NumberOfTarget );
	}

	public void LoadScene( SceneFile scene )
	{
		if ( scene is null )
		{
			Log.Warning( "Attempted to load a null scene." );
			return;
		}

		Log.Info( $"Loading scene: {scene.ResourcePath}" );
		Scene.Load( scene );
		GameObject.StopAllSounds();
	}

	public void LoadMainMenu()
	{
		LoadScene( MainMenuScene );
		
		
	}

	public void LoadGame()
	{
		LoadScene( GameScene );
	}
}
