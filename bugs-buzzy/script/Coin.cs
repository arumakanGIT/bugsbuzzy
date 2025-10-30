using Godot;
using System;

public partial class Coin : Area2D
{
	
	public override void _PhysicsProcess(double delta)
	{
  
		var overlappingBodies = GetOverlappingBodies();

		foreach (var body in overlappingBodies)
		{
			if (body.HasMethod("takeCoins"))
			{ 
				body.Call("takeCoins");
			}
			QueueFree();
		}
	}
}
