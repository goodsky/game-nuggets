using Godot;

public partial class Main : Node
{
	private static readonly string PlayerName = "Player";
	private static readonly string StartPositionName = "StartPosition";
	private static readonly string StartTimerName = "StartTimer";
	private static readonly string ScoreTimerName = "ScoreTimer";
	private static readonly string MobTimerName = "MobTimer";
	private static readonly string MobSpawnLocationName = "MobPath/MobSpawnLocation";

	[Export]
	public PackedScene MobScene { get; set; }

	private int _score;

	public override void _Ready()
	{
		NewGame();
	}

	public override void _Process(double delta)
	{
	}

	public void GameOver()
	{
		GetNode<Timer>(ScoreTimerName).Stop();
		GetNode<Timer>(MobTimerName).Stop();
	}

	public void NewGame()
	{
		_score = 0;

		var player = GetNode<Player>(PlayerName);
		var startPosition = GetNode<Marker2D>(StartPositionName);
		player.Start(startPosition.Position);

		GetNode<Timer>(StartTimerName).Start();
	}

	private void OnStartTimerTimeout()
	{
		GetNode<Timer>(ScoreTimerName).Start();
		GetNode<Timer>(MobTimerName).Start();
	}

	private void OnScoreTimerTimeout()
	{
		_score++;
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

		var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
		mob.LinearVelocity = velocity.Rotated(rotation);

		AddChild(mob);
	}
}