using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public AttackComponent AttackComponent { get; set; }

    [Export]
    public float DashSpeedBoost { get; set; } = 2.0f;

    [Export]
    private float DashTime { get; set; } = 250.0f;

    [Export]
    private float DashDelay { get; set; } = 150.0f;

    // Dashing Related Fields
    private bool IsDashing { get; set; } = false;
    private DateTime DashStartTime { get; set; } = DateTime.MinValue;
    private DateTime LastDashEndTime { get; set; } = DateTime.MinValue;

    [Export]
    public MovementComponent MovementComponent { get; set; }

    public void GetInput()
    {
        Vector2 inputDirection = Input.GetVector("MoveLeft", "MoveRight", "MoveUp", "MoveDown");
        Velocity = inputDirection * MovementComponent.Speed;

        if (Input.IsActionJustPressed("Dash") && !IsDashing && CanDash())
        {
            DashStartTime = DateTime.Now;
            LastDashEndTime = DashStartTime.AddMilliseconds(DashTime);
            IsDashing = true;
        }

        if (IsDashing && Velocity.LengthSquared() > 0)
        {
            Vector2 direction = Velocity.Normalized();
            Velocity = direction * MovementComponent.Speed * DashSpeedBoost;
        }

        if (Input.IsActionJustPressed("Attack_Light"))
        {
            AttackComponent.PerformAttack(0);
        }
        else if (Input.IsActionJustPressed("Attack_Medium"))
        {
            AttackComponent.PerformAttack(1);
        }
        else if (Input.IsActionJustPressed("Attack_Heavy"))
        {
            AttackComponent.PerformAttack(2);
        }
        else if (Input.IsActionJustPressed("Attack_Transition"))
        {
            AttackComponent.PerformTransitionAttack();
        }
    }

    private bool CanDash()
    {
        return DateTime.Now >= LastDashEndTime.AddMilliseconds(DashDelay);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDashing && (DateTime.Now - DashStartTime).TotalMilliseconds >= DashTime)
        {
            IsDashing = false;
        }

        GetInput();
    }
}
