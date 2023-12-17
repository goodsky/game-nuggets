using Godot;
using System;

public partial class Mob : RigidBody2D
{
	public override void _Ready()
	{
		var animatedSprite2d = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		var animations = animatedSprite2d.SpriteFrames.GetAnimationNames();
		animatedSprite2d.Play(animations[GD.Randi() % animations.Length]);
	}

	private void _OnScreenExited()
	{
		QueueFree();
	}
}
