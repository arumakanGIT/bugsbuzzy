// Coin.cs — Option A: reparent the AudioStreamPlayer2D and play
using Godot;
using System;

public partial class Coin : Area2D
{
	private AudioStreamPlayer2D pickupSound;

	public override void _Ready()
	{
		pickupSound = GetNodeOrNull<AudioStreamPlayer2D>("PickupSound");
	}

	public override void _PhysicsProcess(double delta)
	{
		var overlappingBodies = GetOverlappingBodies();

		foreach (var body in overlappingBodies)
		{
			if (body.HasMethod("takeCoins"))
			{
				body.Call("takeCoins");

				if (pickupSound != null && pickupSound.Stream != null)
				{
					var ps = pickupSound;
					RemoveChild(ps); // remove from coin
					GetTree().Root.AddChild(ps); // add to scene root so it persists
					ps.GlobalPosition = GlobalPosition; // optional for 2D positional audio
					ps.Play();

					
					var t = new Timer();
					t.OneShot = true;
					t.WaitTime = 1.0f; // change to clip length if you want exact
					ps.AddChild(t);
					t.Timeout += () =>
					{
						ps.QueueFree();
					};
					t.Start();
				}
				else
				{
					GD.PrintErr("Coin: pickupSound missing or no stream assigned!");
				}

				QueueFree();
				break;
			}
		}
	}
}
