using Godot;

public partial class Main : Node
{
	private static readonly string PlayerName = "Player";
	private static readonly string StartPositionName = "StartPosition";
	private static readonly string StartTimerName = "StartTimer";
	private static readonly string ScoreTimerName = "ScoreTimer";
	private static readonly string MobTimerName = "MobTimer";
	private static readonly string MobSpawnLocationName = "MobPath/MobSpawnLocation";
	private static readonly string HUDName = "HUD";
	private static readonly string MusicName = "Music";
	private static readonly string DeathSoundName = "DeathSound";

	private const double InitialMobTimerWaitTime = 1.5;
	private const double InitialMobMinVelocity = 100.0;
	private const double InitialMobMaxVelocity = 200.0;

	private const double MobTimerWaitTimeGrowthRate = 0.99;
	private const double MobVelocityGrowthRate = 1.01;

	[Export]
	public PackedScene MobScene { get; set; }

	private int _score;

	private double _mobTimerWaitTime;
	private double _mobMinVelocity;
	private double _mobMaxVelocity;

	public override void _Ready() { }

	public override void _Process(double delta) { }

	public void NewGame()
	{
		_score = 0;
		_mobTimerWaitTime = InitialMobTimerWaitTime;
		_mobMinVelocity = InitialMobMinVelocity;
		_mobMaxVelocity = InitialMobMaxVelocity;

		GetTree().CallGroup("mobs", Node.MethodName.QueueFree);

		var player = GetNode<Player>(PlayerName);
		var startPosition = GetNode<Marker2D>(StartPositionName);
		player.Start(startPosition.Position);

		var hud = GetNode<HUD>(HUDName);
		hud.UpdateScore(_score);
		hud.ShowMessage("Get Ready!");

		GetNode<Timer>(StartTimerName).Start();
	}

	public void GameOver()
	{
		GetNode<Timer>(ScoreTimerName).Stop();
		GetNode<Timer>(MobTimerName).Stop();
		GetNode<HUD>(HUDName).ShowGameOver();

		GetNode<AudioStreamPlayer>(MusicName).Stop();
		GetNode<AudioStreamPlayer>(DeathSoundName).Play();
	}

	private void OnStartTimerTimeout()
	{
		var mobTimer = GetNode<Timer>(MobTimerName);
		mobTimer.WaitTime = _mobTimerWaitTime;
		mobTimer.Start();

		GetNode<Timer>(ScoreTimerName).Start();

		GetNode<AudioStreamPlayer>(MusicName).Play();
	}

	private void OnScoreTimerTimeout()
	{
		_score++;
		GetNode<HUD>(HUDName).UpdateScore(_score);
	}

	private void OnMobTimerTimeout()
	{
		var mob = MobScene.Instantiate<Mob>();

		var spawnLocation = GetNode<PathFollow2D>(MobSpawnLocationName);
		spawnLocation.ProgressRatio = GD.Randf();

		var rotation = spawnLocation.Rotation + Mathf.Pi / 2;
		rotation += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);

		mob.Position = spawnLocation.Position;
		mob.Rotation = rotation;

		var velocity = new Vector2((float)GD.RandRange(_mobMinVelocity, _mobMaxVelocity), 0);
		mob.LinearVelocity = velocity.Rotated(rotation);

		AddChild(mob);

		var mobTimer = GetNode<Timer>(MobTimerName);
		mobTimer.WaitTime = _mobTimerWaitTime;

		_mobTimerWaitTime *= MobTimerWaitTimeGrowthRate;
		_mobMinVelocity *= MobVelocityGrowthRate;
		_mobMaxVelocity *= MobVelocityGrowthRate;
	}
}