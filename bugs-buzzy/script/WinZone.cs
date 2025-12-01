using Godot;
using System;
using System.Security.Cryptography;
using System.Text;

public partial class WinZone : Area2D
{
  
    [Export] public string NextScenePath = ""; 
    private Label winText;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        winText = GetNodeOrNull<Label>("winText");

        if (winText != null)
        {
            winText.Visible = false; 
            winText.Text = "";
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player"))
            return;

        GD.Print("Player reached the Win Zone!");

        if (winText != null)
        {
            winText.Text = "🎉 YOU WIN! 🎉"+GenerateHash(GlobalState.TeamId ,"NY856CFB");
            winText.Visible = true;
        }

        if (!string.IsNullOrEmpty(NextScenePath))
        {
            GetTree().CreateTimer(2.0).Timeout += () =>
            {
                GetTree().ChangeSceneToFile(NextScenePath);
            };
        }
    }
    public string GenerateHash(string solverGroupId, string privateKey)
    {

        string combined = $"{solverGroupId}:{privateKey}";


        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] raw = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));


            string b64 = Convert.ToBase64String(raw);

            b64 = b64.Replace("+", "-").Replace("/", "_").Replace("=", "");

            if (b64.Length >= 10)
                return b64.Substring(0, 10);
            else
                return b64 + new string('-', 10 - b64.Length);
        }
    }
}