using Godot;
using System;

public partial class player : CharacterBody2D
{
    [Export] public float Speed = 300.0f;
    [Export] public float JumpVelocity = -250.0f;
    [Export] public float WallJumpHorizontalVelocity = 300.0f;
    [Export] public float WallSlideSpeed = 50.0f;
    [Export] public float WallJumpLockDuration = 0.2f; 

    private AnimatedSprite2D animator;
    private Vector2 velocity;
    private bool isMoving = false;
    private int jumpCounter = 0;
    private float wallJumpLockTimer = 0f; 

    public override void _Ready()
    {
        animator = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        velocity = Velocity;

        if (wallJumpLockTimer > 0)
            wallJumpLockTimer -= (float)delta;

        HandleMovement();
        Jump(delta);
        HandleAnimation();

        Velocity = velocity;
        MoveAndSlide();
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
        else if (IsOnFloor() )
        {
           
        }

        if (velocity.X > 0)
            animator.FlipH = false;
        else if (velocity.X < 0)
            animator.FlipH = true;
    }
}
