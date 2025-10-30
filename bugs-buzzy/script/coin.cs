using Godot;
using System;

public partial class Coin : Area2D
{
    private AnimatedSprite2D sprite;
    private AudioStreamPlayer2D pickupSound;

    public override void _Ready()
    {
    
        sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        pickupSound = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");

  
        Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
    }

    private void OnBodyEntered(Node body)
    {
    
        if (body.IsInGroup("player"))
        {
            GD.Print("Coin picked up!");

        
            if (pickupSound != null)
                pickupSound.Play();

            if (sprite != null)
                sprite.Play("pickup");


            QueueFree();
        }
    }
}