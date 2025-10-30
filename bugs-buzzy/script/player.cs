using Godot;
using System;

public partial class player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -250.0f;
	[Export] public float WallJumpHorizontalVelocity = 300.0f;
	[Export] public float WallSlideSpeed = 50.0f;
	[Export] public float WallJumpLockDuration = 0.2f; 
	[Export] public PackedScene ProjectileScene; 
	[Export] public int SwordDamage = 25;
	
	private AnimatedSprite2D animator;
	private Vector2 velocity;
	private bool isMoving = false;
	private int jumpCounter = 0;
	private float wallJumpLockTimer = 0f; 
	public int Health = 100;
	private Area2D swordHitbox;
	private bool isAttacking = false;
	public void TakeDamage(int damage)
	{
		Health -= damage;
		GD.Print("Player took " + damage + " damage! Health = " + Health);

		if (Health <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Player is dead!");


		GetTree().CreateTimer(0.5f).Timeout += () =>
		{
			GetTree().ReloadCurrentScene();
		};

		Hide(); 
		SetProcess(false);
		SetPhysicsProcess(false);
	}

	public override void _Ready()
	{
		animator = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		swordHitbox = GetNode<Area2D>("SwordHitbox");
		swordHitbox.Monitoring = false;
		swordHitbox.BodyEntered += OnSwordHit;
	}

	public override void _PhysicsProcess(double delta)
	{
		velocity = Velocity;

		if (wallJumpLockTimer > 0)
			wallJumpLockTimer -= (float)delta;

		HandleMovement();
		Jump(delta);
		HandleAnimation();
		HandleInput();

		Velocity = velocity;
		MoveAndSlide();
	}
	private void HandleInput()
	{
		if (Input.IsActionJustPressed("attack_melee") && !isAttacking)
		{
			StartMeleeAttack();
		}

		if (Input.IsActionJustPressed("attack_ranged"))
		{
			ShootProjectile();
		}
	}

	private void HandleMovement()
	{

		if (wallJumpLockTimer > 0)
			return;

		Vector2 direction = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
		if (direction != Vector2.Zero)
		{
			isMoving = true;
			velocity.X = direction.X * Speed;
		}
		else
		{
			isMoving = false;
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
	}

	private void Jump(double delta)
	{
  
		if (!IsOnFloor())
		{
 
			if (IsOnWall() && velocity.Y > 0)
			{
				velocity.Y = Mathf.Min(velocity.Y, WallSlideSpeed);
			   // animator.Play("wall_slide"); 
			}
			else
			{
				velocity += GetGravity() * (float)delta;
			}
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (IsOnFloor())
			{
				jumpCounter++;
				velocity.Y = JumpVelocity;
			}
			else if (IsOnWall())
			{
			  
				int wallDir = GetWallNormal().X > 0 ? 1 : -1;

				velocity.Y = JumpVelocity;
				velocity.X = wallDir * WallJumpHorizontalVelocity;

				jumpCounter = 1;

				wallJumpLockTimer = WallJumpLockDuration;
			}
		}

		if (IsOnFloor())
		{
			jumpCounter = 0;
		}
	}

	private void HandleAnimation()
	{
		if (velocity.Y > 0)
		{
			animator.Play("fall");
		}
		else if (velocity.Y < 0)
		{
			animator.Play("jump");
		}
		else if (IsOnFloor() )
		{
			if (isMoving)
			{
				animator.Play("walk");
			}
			else
			{
				animator.Play("idle"); 
			}
			
		}

		if (velocity.X > 0)
			animator.FlipH = false;
		else if (velocity.X < 0)
			animator.FlipH = true;
		
		if (swordHitbox != null)
		{
	  
			float offsetX = 150f; 
			swordHitbox.Position = new Vector2(animator.FlipH ? -offsetX : offsetX, swordHitbox.Position.Y);
		}
	}
	private async void StartMeleeAttack()
	{
		GD.Print("attack started");
		isAttacking = true;
	   // animator.Play("attack"); 
		swordHitbox.Monitoring = true;

		await ToSignal(GetTree().CreateTimer(0.3f), "timeout");

		swordHitbox.Monitoring = false;
		isAttacking = false;
	}

	private void OnSwordHit(Node body)
	{
		
			if (body.HasMethod("TakeDamage"))
			{
				body.Call("TakeDamage", SwordDamage);
			}
		
	}
	private void ShootProjectile()
	{
		if (ProjectileScene == null) return;

		var projectile = (Projectile)ProjectileScene.Instantiate();

		Vector2 mousePos = GetGlobalMousePosition();

		Vector2 dir = (mousePos - GlobalPosition).Normalized();

		projectile.GlobalPosition = GlobalPosition + dir * 20;

		projectile.SetDirection(dir);

		GetTree().CurrentScene.AddChild(projectile);

		// animator.Play("shoot");
	}


}
