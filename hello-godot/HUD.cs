using Godot;
using System;

public partial class HUD : CanvasLayer
{
	private static readonly string ScoreLabelName = "ScoreLabel";
	private static readonly string MessageLabelName = "MessageLabel";
	private static readonly string MessageTimerName = "MessageTimer";
	private static readonly string StartButtonName = "StartButton";

	[Signal]
	public delegate void StartGameEventHandler();

	public override void _Ready() {}

	public override void _Process(double delta) {}

	public void ShowMessage(string text)
	{
		var messageLabel = GetNode<Label>(MessageLabelName);
		messageLabel.Text = text;
		messageLabel.Show();

		GetNode<Timer>(MessageTimerName).Start();
	}

	public async void ShowGameOver()
	{
		ShowMessage("Game Over");

		var messageTimer = GetNode<Timer>(MessageTimerName);
		await ToSignal(messageTimer, Timer.SignalName.Timeout);

		ShowMessage("Dodge the Creeps!");
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

		GetNode<Button>(StartButtonName).Show();
	}

	public void UpdateScore(int score)
	{
		GetNode<Label>(ScoreLabelName).Text = score.ToString();
	}

	public void OnStartButtonPressed()
	{
		GetNode<Label>(MessageLabelName).Hide();
		GetNode<Button>(StartButtonName).Hide();

		EmitSignal(SignalName.StartGame);
	}

	public void OnMessageTimerTimeout()
	{
		GetNode<Label>(MessageLabelName).Hide();
	}
}
