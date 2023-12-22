using Godot;

public partial class Player : Area2D
{
	private static readonly string CollisionShape2DName = "CollisionShape2D";
	private static readonly string AnimatedSprite2DName = "AnimatedSprite2D";

	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed { get; set; } = 400;

	public Vector2 ScreenSize;

	public override void _Ready()
	{
		Hide();

		ScreenSize = GetViewportRect().Size;
	}

	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;
		if (Input.IsActionPressed("move_up"))
		{
			velocity.Y -= 1;
		}

		if (Input.IsActionPressed("move_down"))
		{
			velocity.Y += 1;
		}

		if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= 1;
		}

		if (Input.IsActionPressed("move_right"))
		{
			velocity.X += 1;
		}

		var animatedSprite = GetNode<AnimatedSprite2D>(AnimatedSprite2DName);
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			animatedSprite.Play();
		}
		else
		{
			animatedSprite.Stop();
		}

		if (velocity.X != 0)
		{
			animatedSprite.Animation = "walk";
			animatedSprite.FlipV = false;
			animatedSprite.FlipH = velocity.X < 0;
		}
		else
		{
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

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		GetNode<CollisionShape2D>(CollisionShape2DName).Set(CollisionShape2D.PropertyName.Disabled, false);
	}

	public void OnBodyEntered(Node2D body)
	{
		Hide();
		EmitSignal(SignalName.Hit);
		GetNode<CollisionShape2D>(CollisionShape2DName).SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}
}
