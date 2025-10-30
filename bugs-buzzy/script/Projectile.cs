using Godot;
using System;

public partial class Projectile : Area2D
{
    [Export] public float Speed = 400f;
    [Export] public int Damage = 20;

    private Vector2 direction = Vector2.Right;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.Normalized();

        var sprite = GetNode<Sprite2D>("Sprite2D");
        if (sprite != null)
        {
           
            sprite.FlipH = dir.X < 0;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        Position += direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("Enemy"))
        {
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", Damage);
            }
        }

        GD.Print(body.Name);

        QueueFree(); 
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
}