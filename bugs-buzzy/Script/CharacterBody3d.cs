using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{
    private const float Speed = 5.0f;
    private const float JumpVelocity = 4.5f;

    private float MouseSensitivity = 0.005f;
    private float pitch = 0.0f;
    private Camera3D camera;

    private int health;
    private int maxHealth = 100;

 
    private float damageCooldown = 1f;
    private float damageTimer = 0f;

    public override void _Ready()
    {
        health = maxHealth;
        Input.MouseMode = Input.MouseModeEnum.Captured;
        camera = GetNode<Camera3D>("Camera3D");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            pitch -= mouseMotion.Relative.Y * MouseSensitivity;
            pitch = Mathf.Clamp(pitch, -1.2f, 1.2f);
            camera.Rotation = new Vector3(pitch, 0, 0);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
 
        if (damageTimer > 0)
            damageTimer -= (float)delta;

        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        Vector2 inputDir = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

  
    public void TakeDamage(int amount)
    {
        if (damageTimer > 0) 
            return; 

        health -= amount;
        GD.Print("Player took damage: ", amount, " | Health: ", health);

        damageTimer = damageCooldown; 

        if (health <= 0)
            Die();
    }

    private void Die()
    {
        GD.Print("You are dead");
      
    }
}
