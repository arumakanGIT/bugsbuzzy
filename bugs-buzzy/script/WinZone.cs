using Godot;
using System;

public partial class WinZone : Area2D
{
    [Export] public string NextScenePath = ""; 

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player"))
            return;

        GD.Print("Player reached the Win Zone!");

        if (!string.IsNullOrEmpty(NextScenePath))
        {
          
            GetTree().ChangeSceneToFile(NextScenePath);
        }
        else
        {
           
            GD.Print("You Win!");
        }
    }
}