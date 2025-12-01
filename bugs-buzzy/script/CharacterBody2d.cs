using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
    [Export] public float Speed = 200.0f;
    [Export] public float PatrolRange = 100.0f;
    [Export] public int Damage = 10;
    [Export] public int Health = 100;

    private Vector2 startPosition;
    private float targetX;
    private int direction = 1;
    private AnimatedSprite2D animator;

    public override void _Ready()
    {
        startPosition = GlobalPosition;
        targetX = startPosition.X + PatrolRange;
        animator = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");

        if (animator != null)
            animator.Play("walk");
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Calculate movement only in X direction
        float moveAmount = Speed * direction * dt;

        // Update position manually (no physics interference)
        GlobalPosition = new Vector2(GlobalPosition.X + moveAmount, startPosition.Y);

        // Check patrol limits
        float distanceFromStart = GlobalPosition.X - startPosition.X;
        if (Mathf.Abs(distanceFromStart) >= PatrolRange)
        {
            direction *= -1;
            GlobalPosition = new Vector2(
                startPosition.X + Mathf.Clamp(distanceFromStart, -PatrolRange, PatrolRange),
                startPosition.Y
            );
        }
    }
}