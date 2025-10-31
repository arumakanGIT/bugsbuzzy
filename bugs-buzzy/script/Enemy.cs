using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float PatrolRange = 500.0f;
	[Export] public int Damage = 10; // مقدار دمیج
	[Export] public int Health = 100;

	private Vector2 startPosition;
	private float targetX;
	private int direction = 1;
	private AnimatedSprite2D animator;
	private Area2D hitBox; // برای تشخیص برخورد با پلیر

	public override void _Ready()
	{
		startPosition = GlobalPosition;
		targetX = startPosition.X + PatrolRange;
		animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		hitBox = GetNode<Area2D>("HitBox");
		hitBox.BodyEntered += OnHitBoxBodyEntered;

		if (animator != null)
			animator.Play("walk");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		float moveAmount = Speed * direction;

		Velocity = new Vector2(moveAmount, Velocity.Y);
		MoveAndSlide();

		if (IsOnWall())
		{
			direction *= -1;
			targetX = startPosition.X + direction * PatrolRange;
		}

		float distanceFromStart = GlobalPosition.X - startPosition.X;
		if (Mathf.Abs(distanceFromStart) >= PatrolRange)
		{
			direction *= -1;
			float clampedX = startPosition.X + Mathf.Clamp(distanceFromStart, -PatrolRange, PatrolRange);
			GlobalPosition = new Vector2(clampedX, GlobalPosition.Y);
			targetX = startPosition.X + direction * PatrolRange;
		}

		if (animator != null)
		{
			animator.FlipH = direction < 0;
		}
	}


	private void OnHitBoxBodyEntered(Node body)
	{
			if (body is player player)
			{
				player.TakeDamage(Damage);
			}
	}

	public void TakeDamage(int amount)
	{
		Health-=amount;
		GD.Print("health: "+Health);
		if (Health <= 0)
		{
			QueueFree();
		}
	}
}
