using Godot;
using System;

public partial class Ui : CanvasLayer
{
    private Label hpLabel;
    private Label soulLabel;

    public override void _Ready()
    {
        hpLabel = GetNode<Label>("HpLabel");
        soulLabel = GetNode<Label>("SoulLabel");
    }

    public void UpdateHP(int currentHP)
    {
        hpLabel.Text = $"HP: {currentHP}";
    }

    public void UpdateSoul(int soul)
    {
        soulLabel.Text = $"Soul: {soul}";
    }
}