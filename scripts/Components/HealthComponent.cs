using Godot;
using System;
using System.Diagnostics;

public partial class HealthComponent : Node2D
{
    public float Health { get; set; } = 100;

    [Export]
    public float MaxHealth { get; set; } = 100;

    [Export]
    public bool HealthRegeneration { get; set; } = true;

    [Export]
    public float HealthRegenerationRate { get; set; } = 2.0f;

    private double TimeSinceLastHealthRegeneration = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        TimeSinceLastHealthRegeneration += delta;
        if (TimeSinceLastHealthRegeneration > 1000)
        {
            AddHealth(HealthRegenerationRate);
        }
    }

    public void AddHealth(float value)
    {
        Health = Mathf.Clamp(Health + value, 0, MaxHealth);

        if (Health <= 0) { }
    }
}
