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
	[Export] public float DashSpeed = 800f;   
	[Export] public float DashDuration = 0.2f; 
	[Export] public float DashCooldown = 0.5f; 

	private bool isDashing = false;
	private float dashTimer = 0f;
	private float dashCooldownTimer = 0f;
	private Vector2 dashDirection = Vector2.Zero;

	
	private AnimatedSprite2D animator;
	private Vector2 velocity;
	private bool isMoving = false;
	private int jumpCounter = 0;
	private float wallJumpLockTimer = 0f; 
	public int Health = 100;
	[Export] public int MaxHealth = 100;
	private float healTimer = 0f;
	private const float healInterval = 0.5f;
	private Area2D swordHitbox;
	private bool isAttacking = false;
	private int soul = 0;
	
	private Ui ui;

	public void TakeDamage(int damage)
	{
		if (!isDashing)
		{
			Health -= damage;
			GD.Print("Player took " + damage + " damage! Health = " + Health);
			ui?.UpdateHP(Health);
			if (Health <= 0)
			{
				Die();
			}
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
		ui = GetTree().CurrentScene.GetNodeOrNull<Ui>("UI");
		if (ui != null)
		{
			ui.UpdateHP(Health);
			ui.UpdateSoul(soul);
		}

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
		HandleHealing(delta);

		if (dashCooldownTimer > 0)
			dashCooldownTimer -= (float)delta;

		if (isDashing)
		{
			dashTimer -= (float)delta;
			velocity = dashDirection * DashSpeed;

			if (dashTimer <= 0)
			{
				isDashing = false;
			}
		}

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
		if (Input.IsActionJustPressed("dash"))
		{
			StartDash();
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

	public void takeCoins()
	{
		soul += 10;
		ui?.UpdateSoul(soul);
	}

	private void HandleHealing(double delta)
	{
		if (Input.IsActionPressed("heal")) 
		{
			healTimer -= (float)delta;
			if (healTimer <= 0f)
			{
				int healAmount = soul / 2; 
				if (healAmount > 0)
				{
					Health += healAmount;
					if (Health > MaxHealth)
						Health = MaxHealth;

					soul -= healAmount * 2; 
					GD.Print($"Healed {healAmount} HP. Current HP: {Health}, Souls left: {soul}");
				}

				healTimer = healInterval;
			}
		}
		else
		{
			healTimer = 0f; 
		}
	}
	private void StartDash()
	{
		if (dashCooldownTimer > 0 || isDashing) return;

		isDashing = true;
		dashTimer = DashDuration;
		dashCooldownTimer = DashCooldown;

		
		Vector2 inputDir = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
		if (inputDir == Vector2.Zero)
		{
			
			inputDir = new Vector2(animator.FlipH ? -1 : 1, 0);
		}

		dashDirection = inputDir.Normalized();
		animator.Play("dash");
		GD.Print("Dash started!");
	}


}
