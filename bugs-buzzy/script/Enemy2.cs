// Enemy2.cs
using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy2 : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float PatrolRange = 50.0f;

	// detection radii: chase when player is <= ChaseRadius, return to patrol when > LoseRadius
	[Export] public float ChaseRadius = 150.0f;
	[Export] public float LoseRadius = 200.0f; // must be >= ChaseRadius for hysteresis

	private Vector2 startPosition;
	private float targetX;
	private int patrolDirection = 1;

	private AnimatedSprite2D animator;
	private CharacterBody2D playerNode;

	private enum State { Patrol, Chase }
	private State state = State.Patrol;

	public override void _Ready()
	{
		startPosition = GlobalPosition;
		targetX = startPosition.X + PatrolRange;
		animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		animator?.Play("walk");

		// safer player lookup: find first node in group "Player"
		var players = GetTree().GetNodesInGroup("Player");
		if (players.Count > 0)
		{
			playerNode = players[0] as CharacterBody2D;
			GD.Print($"Enemy found player node: {playerNode.Name}");
		}
		else
		{
			playerNode = null;
			GD.PrintErr("Enemy: Player node not found. Make sure the player is in the 'Player' group.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// update state with hysteresis
		if (playerNode != null)
		{
			float distToPlayer = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
			if (state == State.Patrol && distToPlayer <= ChaseRadius)
			{
				state = State.Chase;
			}
			else if (state == State.Chase && distToPlayer > LoseRadius)
			{
				// lost the player -> return to patrol
				state = State.Patrol;
				// re-calculate patrol target based on current direction and start
				patrolDirection = (GlobalPosition.X < startPosition.X) ? 1 : -1;
				targetX = startPosition.X + patrolDirection * PatrolRange;
			}
		}

		if (state == State.Chase && playerNode != null)
		{
			ChasePlayer((float)delta);
		}
		else
		{
			Patrol((float)delta);
		}
	}

	private void ChasePlayer(float dt)
	{
		// compute normalized direction toward player so speed is consistent in both axes
		Vector2 toPlayer = playerNode.GlobalPosition - GlobalPosition;

		// if very close, stop moving
		const float stopDistance = 4.0f;
		if (toPlayer.Length() <= stopDistance)
		{
			Velocity = new Vector2(0, 0);
			MoveAndSlide();
			return;
		}

		Vector2 dir = toPlayer.Normalized();
		Velocity = dir * Speed;

		// facing: flip horizontally by X direction
		if (animator != null)
			animator.FlipH = dir.X < 0;

		MoveAndSlide();
	}

	private void Patrol(float dt)
	{
		// move horizontally; keep Y velocity zero (or you could add gravity if desired)
		float moveAmount = Speed * patrolDirection;
		Velocity = new Vector2(moveAmount, 0);
		MoveAndSlide();

		// check if near targetX; use dt step to prevent overshoot toggling
		float distToTarget = GlobalPosition.X - targetX;
		float step = Mathf.Abs(moveAmount * dt);

		if (Mathf.Abs(distToTarget) <= step)
		{
			// snap exactly to target and flip direction
			GlobalPosition = new Vector2(targetX, GlobalPosition.Y);

			patrolDirection *= -1;
			targetX = startPosition.X + patrolDirection * PatrolRange;
		}

		// update sprite flip for patrol
		if (animator != null)
			animator.FlipH = patrolDirection < 0;
	}

	// optional: collision reaction with player via hitbox area
	private void _on_Hitbox_body_entered(Node body)
	{
		if (body.IsInGroup("Player"))
		{
			GD.Print("Player hit!");
			// handle damage / knockback here
		}
	}
}
