// Enemy4.cs
using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy3 : CharacterBody2D
{
	[Export] public float WalkSpeed = 80.0f;
	[Export] public float ChaseSpeed = 140.0f;
	[Export] public float DashSpeed = 360.0f;
	[Export] public float DashDuration = 0.25f;
	[Export] public float DashCooldown = 1.5f;
	[Export] public float Gravity = 900.0f;
	[Export] public int Health = 100;

	[Export] public float ChaseRadius = 160.0f;
	[Export] public float LoseRadius = 220.0f;
	[Export] public float DashRange = 48.0f;

	private AnimatedSprite2D animator;
	private RayCast2D rayGround;
	private RayCast2D rayWall;
	private CharacterBody2D playerNode;

	private int direction = -1;
	private float dashTimer = 0.0f;
	private float dashCooldownTimer = 0.0f;

	private enum State { Patrol, Chase, Dashing }
	private State state = State.Patrol;

	public override void _Ready()
	{
		animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		rayGround = GetNodeOrNull<RayCast2D>("RayCastGround");
		rayWall = GetNodeOrNull<RayCast2D>("RayCastWall");

		if (rayGround != null) rayGround.Enabled = true;
		if (rayWall != null) rayWall.Enabled = true;

		var players = GetTree().GetNodesInGroup("Player");
		if (players.Count > 0)
			playerNode = players[0] as CharacterBody2D;
		else
			playerNode = null;

		if (animator != null)
			animator.FlipH = direction < 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// gravity (modify via local copy to avoid CS1612)
		var v = Velocity;
		if (!IsOnFloor())
			v.Y += Gravity * dt;
		else
			v.Y = 0;
		Velocity = v;

		// timers
		if (dashCooldownTimer > 0) dashCooldownTimer = Mathf.Max(0, dashCooldownTimer - dt);
		if (dashTimer > 0) dashTimer = Mathf.Max(0, dashTimer - dt);

		// state hysteresis
		if (playerNode != null)
		{
			float dist = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
			if (state == State.Patrol && dist <= ChaseRadius)
				state = State.Chase;
			else if (state == State.Chase && dist > LoseRadius)
				state = State.Patrol;
		}

		switch (state)
		{
			case State.Patrol:
				DoPatrol(dt);
				break;
			case State.Chase:
				DoChase(dt);
				break;
			case State.Dashing:
				DoDash(dt);
				break;
		}

		if (animator != null)
			animator.FlipH = direction < 0;

		MoveAndSlide();
	}

	private void DoPatrol(float dt)
	{
		bool shouldFlip = false;

		// rayGround and rayWall assume you placed them ahead in the scene
		if (rayGround != null)
		{
			// If the ground-ray is not colliding, it's an edge -> flip
			if (!rayGround.IsColliding())
				shouldFlip = true;
		}

		if (rayWall != null && rayWall.IsColliding())
			shouldFlip = true;

		if (IsOnWall())
			shouldFlip = true;

		if (shouldFlip)
			FlipDirection();

		// compute desired movement and smooth
		float targetSpeed = WalkSpeed * direction;
		Vector2 desired = new Vector2(targetSpeed, Velocity.Y);
		Velocity = Velocity.Lerp(desired, 12.0f * dt);

		//animator?.Play("walk");
	}

	private void DoChase(float dt)
	{
		if (playerNode == null)
		{
			state = State.Patrol;
			return;
		}

		Vector2 toPlayer = playerNode.GlobalPosition - GlobalPosition;
		direction = toPlayer.X < 0 ? -1 : 1;

		if (toPlayer.Length() <= DashRange && dashCooldownTimer <= 0)
		{
			StartDash();
			return;
		}

		float desiredX = Mathf.Sign(toPlayer.X) * ChaseSpeed;
		Vector2 desired = new Vector2(desiredX, Velocity.Y);
		Velocity = Velocity.Lerp(desired, 8.0f * dt);

		//animator?.Play("run");
	}

	private void StartDash()
	{
		state = State.Dashing;
		dashTimer = DashDuration;
		dashCooldownTimer = DashCooldown;
		Velocity = new Vector2(direction * DashSpeed, 0);
		//animator?.Play("dash");
	}

	private void DoDash(float dt)
	{
		Velocity = Velocity.Lerp(new Vector2(direction * DashSpeed, 0), 20.0f * dt);

		if (dashTimer <= 0 || IsOnWall())
		{
			state = State.Chase;
			Velocity = new Vector2(direction * WalkSpeed * 0.5f, Velocity.Y);
		}
	}

	private void FlipDirection()
	{
		direction *= -1;

		// adjust ray target positions (use TargetPosition in Godot 4)
		if (rayGround != null)
		{
			Vector2 tp = rayGround.TargetPosition;
			tp.X = Mathf.Abs(tp.X) * direction;
			rayGround.TargetPosition = tp;
		}
		if (rayWall != null)
		{
			Vector2 tp = rayWall.TargetPosition;
			tp.X = Mathf.Abs(tp.X) * direction;
			rayWall.TargetPosition = tp;
		}
	}

	private void _on_Hitbox_body_entered(Node body)
	{
		if (body.IsInGroup("Player"))
			GD.Print("Enemy4 hit player");
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
