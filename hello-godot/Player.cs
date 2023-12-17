using Godot;

public partial class Player : Area2D
{
	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed { get; set; } = 400;

	public Vector2 ScreenSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;	
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;
		if (Input.IsActionPressed("move_up")) {
			velocity.Y -= 1;
		}

		if (Input.IsActionPressed("move_down")) {
			velocity.Y += 1;
		}

		if (Input.IsActionPressed("move_left")) {
			velocity.X -= 1;
		}

		if (Input.IsActionPressed("move_right")) {
			velocity.X += 1;
		}

		var animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if (velocity.Length() > 0) {
			velocity = velocity.Normalized() * Speed;
			animatedSprite.Play();
		} else {
			animatedSprite.Stop();
		}

		if (velocity.X != 0) {
			animatedSprite.Animation = "walk";
			animatedSprite.FlipV = false;
			animatedSprite.FlipH = velocity.X < 0;
		} else {
			animatedSprite.Animation = "up";
			animatedSprite.FlipH = false;
			animatedSprite.FlipV = velocity.Y > 0;
		}

		Position += velocity * (float)delta;
		Position = new Vector2(
			Mathf.Clamp(Position.X, 0.0f, ScreenSize.X),
			Mathf.Clamp(Position.Y, 0.0f, ScreenSize.Y)
		);
	}
}
