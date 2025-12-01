using Godot;
using System;

public partial class GlobalState : Node
{
    public static string TeamId ;

    public void setTeamId(string id)
    {
        TeamId = id;
    }
}