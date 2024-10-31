using Godot;
using System;

public partial class MovementComponent : Node2D
{
    [Export]
    public CharacterBody2D Character;

    [Export]
    public float Friction = 500.0f;

    public int Speed { get; set; } = 1000;
    private float originalSpeed;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        originalSpeed = Speed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        // Apply friction to gradually reduce speed to zero when not moving
        if (Character.Velocity.Length() > 0)
        {
            Character.Velocity = Character.Velocity.MoveToward(
                Vector2.Zero,
                Friction * (float)delta
            );
        }

        Character.MoveAndSlide();
    }

    // Reduce speed during the attack
    public void ReduceSpeed(float duration, float power)
    {
        Speed = (int)(originalSpeed / power);
        GetTree()
            .CreateTimer(duration)
            .Connect("timeout", Callable.From(() => Speed = (int)originalSpeed));
    }

    public void Stun(float duration)
    {
        if (Character is Player)
        {
            (GetTree().Root.GetNode("RootApp/GUI").GetChild(0) as Hud).EmitSignal(
                Hud.SignalName.PlayerStunned,
                Character,
                duration
            );
        }
        Character.SetPhysicsProcess(false);
        GetTree()
            .CreateTimer(duration)
            .Connect("timeout", Callable.From(() => Character.SetPhysicsProcess(true)));
    }
}
