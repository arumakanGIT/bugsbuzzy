// Enemy3.cs
using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy3 : CharacterBody2D
{
	// Movement
	[Export] public float Speed = 160.0f;          // max movement speed (pixels/sec)
	[Export] public float Acceleration = 8.0f;     // how quickly velocity approaches target
	[Export] public float PatrolRange = 120.0f;    // horizontal patrol radius from start
	[Export] public float HoverAmplitude = 12.0f;  // vertical hover amount (pixels)
	[Export] public float HoverSpeed = 2.0f;       // vertical hover speed (radians/sec)

	// Detection / hysteresis
	[Export] public float ChaseRadius = 160.0f;
	[Export] public float LoseRadius = 220.0f;     // must be >= ChaseRadius

	private Vector2 startPosition;
	private Vector2 patrolTarget;       // current patrol target x
	private int patrolDirection = 1;    // 1 => right, -1 => left

	private AnimatedSprite2D animator;
	private CharacterBody2D playerNode;

	private float hoverTimer = 0.0f;

	private enum State { Patrol, Chase }
	private State state = State.Patrol;

	public override void _Ready()
	{
		startPosition = GlobalPosition;
		patrolTarget = startPosition + new Vector2(PatrolRange, 0);
		animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

		// safer player lookup: find first node in group "Player"
		var players = GetTree().GetNodesInGroup("Player");
		if (players.Count > 0)
		{
			playerNode = players[0] as CharacterBody2D;
			GD.Print($"Enemy3 found player node: {playerNode.Name}");
		}
		else
		{
			playerNode = null;
			GD.PrintErr("Enemy3: Player node not found. Make sure the player is in the 'Player' group.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		hoverTimer += HoverSpeed * dt;

		// update state with hysteresis (only if we have a player)
		if (playerNode != null)
		{
			float distToPlayer = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
			if (state == State.Patrol && distToPlayer <= ChaseRadius)
				state = State.Chase;
			else if (state == State.Chase && distToPlayer > LoseRadius)
			{
				state = State.Patrol;
				// recalc patrol direction based on current position
				patrolDirection = (GlobalPosition.X < startPosition.X) ? 1 : -1;
				patrolTarget = startPosition + new Vector2(patrolDirection * PatrolRange, 0);
			}
		}

		if (state == State.Chase && playerNode != null)
		{
			DoChase(dt);
		}
		else
		{
			DoPatrol(dt);
		}

		MoveAndSlide();
	}

	private void DoChase(float dt)
	{
		// Move directly towards the player's global position (free 2D flying)
		Vector2 toPlayer = playerNode.GlobalPosition - GlobalPosition;
		const float stopDistance = 6.0f;
		if (toPlayer.Length() <= stopDistance)
		{
			// close enough: decelerate to stop
			Velocity = Velocity.Lerp(Vector2.Zero, Acceleration * dt);
			UpdateAnimation(Vector2.Zero);
			return;
		}

		Vector2 desired = toPlayer.Normalized() * Speed;
		Velocity = Velocity.Lerp(desired, Acceleration * dt);

		UpdateAnimation(desired);
	}

	private void DoPatrol(float dt)
	{
		// Horizontal patrol: move toward patrolTarget.X, with vertical hover using sine wave
		float step = Speed * dt;
		float currentX = GlobalPosition.X;
		float distToTargetX = patrolTarget.X - currentX;

		// If close enough to target, snap and flip direction
		if (Mathf.Abs(distToTargetX) <= step)
		{
			GlobalPosition = new Vector2(patrolTarget.X, GlobalPosition.Y);
			patrolDirection *= -1;
			patrolTarget = startPosition + new Vector2(patrolDirection * PatrolRange, 0);
		}

		// desired horizontal velocity towards target
		float desiredX = distToTargetX == 0 ? 0 : Mathf.Sign(distToTargetX) * Speed;

		// vertical hovering (offset from start position)
		float hoverOffset = Mathf.Sin(hoverTimer) * HoverAmplitude;
		float desiredY = (startPosition.Y + hoverOffset) - GlobalPosition.Y; // desired delta to target hover point
		// convert desiredY into velocity (clamped)
		float desiredYVel = Mathf.Clamp(desiredY * 4.0f, -Speed, Speed);

		Vector2 desired = new Vector2(desiredX, desiredYVel);
		Velocity = Velocity.Lerp(desired, Acceleration * dt);

		UpdateAnimation(desired);
	}

	private void UpdateAnimation(Vector2 desiredVelocity)
	{
		// Optional: play walk/fly animation and flip sprite horizontally based on X
		if (animator != null)
		{
			if (desiredVelocity.Length() > 1.0f)
			{
				if (!animator.IsPlaying())
					animator.Play("walk");
			}
			else
			{
				if (!animator.IsPlaying() || animator.Animation != "idle")
					animator.Play("idle");
			}

			// flip horizontally according to X direction
			if (desiredVelocity.X != 0)
				animator.FlipH = desiredVelocity.X < 0;
		}
	}

	// optional: signal handler if you use an Area2D hitbox
	private void _on_Hitbox_body_entered(Node body)
	{
		if (body.IsInGroup("Player"))
		{
			GD.Print("Enemy3 hit the player!");
			// apply damage/knockback here
		}
	}
}
