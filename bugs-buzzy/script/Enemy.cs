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
		// fast local copy and cast to float once
		float dt = (float)delta;
		float moveAmount = Speed * direction;

		// Update velocity (units per second). CharacterBody2D.MoveAndSlide uses this in physics.
		Velocity = new Vector2(moveAmount, Velocity.Y);

		// Move — keep using MoveAndSlide for sliding behavior
		MoveAndSlide();

		// ---------- improved endpoint check (prevents overshoot jitter) ----------
		// compute distance left to target on X
		float distToTarget = GlobalPosition.X - targetX; // positive if past target
		// If moving right (dir==1) and we're within one step of reaching target, snap and reverse.
		// Using Mathf.Abs avoids extra branching.
		float step = Mathf.Abs(moveAmount * dt); // how far we'd move this frame (approx)
		if (Mathf.Abs(distToTarget) <= step)
		{
			// snap to target exactly once then reverse
			GlobalPosition = new Vector2(targetX, GlobalPosition.Y);

			// flip direction and calculate next target (no jitter next frames)
			direction *= -1;
			targetX = startPosition.X + direction * PatrolRange;
		}

		// Flip sprite if present (only set when it actually changes to avoid extra property writes)
		if (animator != null)
		{
			bool flip = direction < 0;
			if (animator.FlipH != flip)
				animator.FlipH = flip;
		}
	}
}
