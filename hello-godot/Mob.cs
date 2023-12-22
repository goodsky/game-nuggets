using Godot;

public partial class Mob : RigidBody2D
{
	private static readonly string AnimatedSprite2DName = "AnimatedSprite2D";

	public override void _Ready()
	{
		var animatedSprite2d = GetNode<AnimatedSprite2D>(AnimatedSprite2DName);
		var animations = animatedSprite2d.SpriteFrames.GetAnimationNames();
		animatedSprite2d.Play(animations[GD.Randi() % animations.Length]);
	}

	private void _OnScreenExited()
	{
		QueueFree();
	}
}
