using Godot;
using System;

public partial class coin : Area2D
{

	private CollisionShape2D collider;
	BodyEnteredEventHandler bodyEntered;
	BodyExitedEventHandler bodyExited;
	public override void _Ready()
	{
		collider = GetNode<CollisionShape2D>("CollisionShape2D");
		Connect("body_entered", new Callable(this , nameof(BodyEntered)));
	}
    
	public override void _Process(double delta)
	{
		
	}
    
   
    
	
}
