using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float PatrolRange = 50.0f;

	private Vector2 startPosition;
	private float targetX;
	private int direction = 1;
	private AnimatedSprite2D animator;

	public override void _Ready()
	{
		startPosition = GlobalPosition;
		targetX = startPosition.X + PatrolRange;
		animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		animator?.Play("walk");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Set velocity first (Velocity is in px/sec for CharacterBody2D).
		Velocity = new Vector2(Speed * direction, Velocity.Y);

		// Apply movement
		MoveAndSlide();

		// Optionally clamp position when we've passed the target to avoid jitter.
		if ((direction == 1 && GlobalPosition.X >= targetX) ||
			(direction == -1 && GlobalPosition.X <= targetX))
		{
			// Snap to exact target X to avoid overshoot and jitter
			GlobalPosition = new Vector2(targetX, GlobalPosition.Y);

			// Flip direction and compute next target
			direction *= -1;
			targetX = startPosition.X + direction * PatrolRange;
		}

		// Flip the sprite by property (safer than using Set)
		if (animator != null)
			animator.FlipH = direction < 0;
	}
}
