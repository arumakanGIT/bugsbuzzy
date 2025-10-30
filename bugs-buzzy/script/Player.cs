using Godot;


public partial class player : CharacterBody2D
{
    [Export] public float Speed = 300.0f;
    [Export] public float JumpVelocity = -250.0f;
    [Export] public float wallJumpHorizantalVelocity = 300.0f;
    private AnimatedSprite2D animator;
    private bool isMoving = false;
    private Vector2 velocity;
    private int jumpCounter = 0;


    public override void _Ready()
    {
        animator = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        velocity = Velocity;

		
		
        handleMovment();
        jump(delta);
        handleAnimation();

        Velocity = velocity;
        MoveAndSlide();
    }

    private void handleMovment()
    {
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

    }
    private void jump(double delta)
    {

        if (!IsOnFloor())
        {
            velocity += GetGravity() * (float)delta;
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
                velocity.X = wallDir * wallJumpHorizantalVelocity;

                jumpCounter = 1;
            }
        }

        if (IsOnFloor())
        {
            jumpCounter = 0;
        }
    }


    private void handleAnimation( )
    {
        if(velocity .Y > 0)
        {
            animator.Play("fall");
        }else if (velocity.Y < 0)
        {
            animator.Play("jump");
        }
        if (velocity.X > 0)
        {
            animator.FlipH = false;
        }
        else if(velocity.X < 0)
        {
            animator.FlipH = true;
        }
    }
}

