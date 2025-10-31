using Godot;
using System;

public partial class KillZone : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
            var player = body as Node; 
            if (player != null)
            {
                player.Call("Die");
                GD.Print("Player died!");
            }
    }
    
}