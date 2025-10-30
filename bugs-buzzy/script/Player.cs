using Godot;
using System;

public partial class Player : CharacterBody2D
{
	  [Export] public float Speed = 600.0f;
	  [Export] public float JumpVelocity = -500.0f;
	  private AnimatedSprite2D animator;
	  private bool isMoving = false;

	  public override void _Ready()
	  {
		 animator = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	  }

	  public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		
	

		Vector2 direction = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
		if (direction != Vector2.Zero)
		{
			isMoving = true;
			velocity.X = direction.X * Speed;
			animator.Play("walk");
		}
		else
		{
			animator.Play("idle");
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (velocity.X > 0)
		{
			animator.FlipH = false;
		}
		else if(velocity.X < 0)
		{
			animator.FlipH = true;
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
