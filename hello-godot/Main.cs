using Godot;
using System;

public partial class Main : Node
{
	private static readonly string PlayerName = "Player";
	private static readonly string StartPositionName = "StartPosition";
	private static readonly string StartTimerName = "StartTimer";
	private static readonly string ScoreTimerName = "ScoreTimer";
	private static readonly string MobTimerName = "MobTimer";

	[Export]
	public PackedScene MobScene { get; set; }

	private int _score;

	public override void _Ready()
	{
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
}