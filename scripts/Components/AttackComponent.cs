using Godot;
using System;
using System.Linq;

public partial class AttackComponent : Area2D
{
	// Mana properties
	public float Mana { get; set; } = 100.0f;

	[Export]
	public float AttackAreaSize { get; set; } = 1.0f;

	[Export]
	public float MaxMana { get; set; } = 100.0f;

	// Combo system properties
	[Export]
	public int MaxComboCount { get; set; } = 0; // Example max combo count

	[Export]
	public float ComboResetTime { get; set; } = 0.3f; // Time to reset the combo chain
	private float baseComboResetTime; // Store the base reset time

	private int currentComboCount = 0;
	private DateTime lastAttackEndTime = DateTime.MinValue;

	[Export]
	public CharacterBody2D Character { get; set; }

	// Attack properties
	[Export]
	public float[] AttackDurations = { 0.5f, 1.0f, 1.5f }; // 0: Light, 1: Medium, 2: Heavy

	[Export]
	public float[] AttackPowers = { 1.0f, 1.5f, 2.0f }; // 0: Light, 1: Medium, 2: Heavy

	[Export]
	public int NumberOfLightAttacks = 3;

	[Export]
	public int NumberOfMediumAttacks = 1;

	[Export]
	public int NumberOfHeavyAttacks = 1;

	private MovementComponent movementComponent;

	// Transition attack flag
	private bool isTransitionAttack = false;

	// Punishment properties
	[Export]
	public float TransitionAttackPunishmentFactor { get; set; } = 1.5f; // Increase the cooldown by 50%

	public GameScene gameScene;

	// Called when the node enters the scene tree for the first time
	public override void _Ready()
	{
		gameScene = GetTree().Root.GetNode("RootApp").GetChild(0) as GameScene;
		// Set up the area size
		var shape = GetNode("AttackArea") as CollisionShape2D;
		(shape.Shape as CircleShape2D).Radius = AttackAreaSize;
		Monitoring = true;

		if (!Monitoring)
		{
			GD.PrintErr("Monitoring is not enabled!");
		}

		movementComponent = Character.GetNode<MovementComponent>("MovementComponent");
		baseComboResetTime = ComboResetTime; // Store the base reset time
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame
	public override void _Process(double delta)
	{
		// Check if the combo should be reset
		if (
			!isTransitionAttack && (DateTime.Now - lastAttackEndTime).TotalSeconds >= ComboResetTime
		)
		{
			currentComboCount = 0;
		}
	}

	// Method to perform an attack
	public void PerformAttack(int attackType)
	{
		if (CanAttack())
		{
			// Determine the expected attack type based on the combo count
			int expectedAttackType = DetermineExpectedAttackType();

			// Check if the performed attack type matches the expected attack type
			if (attackType != expectedAttackType && !isTransitionAttack)
			{
				ResetCombo();
				return;
			}

			if (currentComboCount > MaxComboCount && MaxComboCount != 0)
			{
				ResetCombo();
			}

			currentComboCount++;

			// Get attack duration and power
			float attackDuration = AttackDurations[attackType];
			float attackPower = AttackPowers[attackType];

			// Restrict movement
			movementComponent.ReduceSpeed(attackDuration * (attackType + 2), 5);

			// Implement your attack logic here
			// For example, checking for enemies within the attack area and applying damage
			AttackEnemies(attackType);

			GD.Print($"Attack performed! Combo: {currentComboCount}");

			// Update the last attack end time
			lastAttackEndTime = DateTime.Now.AddSeconds(attackDuration);
		}
	}

	// Method to determine the expected attack type based on the combo count
	private int DetermineExpectedAttackType()
	{
		int totalAttacks = NumberOfLightAttacks + NumberOfMediumAttacks + NumberOfHeavyAttacks;
		if (currentComboCount % totalAttacks < NumberOfLightAttacks)
		{
			return 0; // Light attack
		}
		else if (currentComboCount % totalAttacks < NumberOfLightAttacks + NumberOfMediumAttacks)
		{
			return 1; // Medium attack
		}
		else
		{
			return 2; // Heavy attack
		}
	}

	// Method to reset the combo
	private void ResetCombo()
	{
		currentComboCount = 0;
		GD.Print("Combo reset due to incorrect attack type.");
	}

	// Method to perform a transition attack
	public void PerformTransitionAttack()
	{
		if (CanPerformTransitionAttack())
		{
			// Check if we are transitioning from the last combo
			int totalAttacks = NumberOfLightAttacks + NumberOfMediumAttacks + NumberOfHeavyAttacks;
			if (
				currentComboCount % totalAttacks
				== NumberOfLightAttacks + NumberOfMediumAttacks + NumberOfHeavyAttacks - 1
			)
			{
				isTransitionAttack = true;
				PerformAttack(0); // Assume transition attack is always a heavy attack
				isTransitionAttack = false;
			}
			else
			{
				// If not transitioning correctly, apply punishment
				ApplyTransitionAttackPunishment();
			}
		}
	}

	// Apply punishment for a mistimed transition attack
	private void ApplyTransitionAttackPunishment()
	{
		ComboResetTime = baseComboResetTime * TransitionAttackPunishmentFactor; // Increase the cooldown time
		movementComponent.Stun(((float)currentComboCount) / 10f);
		GD.Print("Transition attack missed! Combo cooldown increased.");
		gameScene.EmitSignal(GameScene.SignalName.CharacterAttackMiss, Character);
		ResetCombo();
	}

	// Check if a transition attack can be performed
	private bool CanPerformTransitionAttack()
	{
		return (DateTime.Now - lastAttackEndTime).TotalSeconds < ComboResetTime;
	}

	// Check if the attack can be performed
	private bool CanAttack()
	{
		return DateTime.Now >= lastAttackEndTime;
	}

	// Method to check for enemies within the attack area
	private void AttackEnemies(int attackType)
	{
		var overlappingBodies = GetOverlappingBodies();
		overlappingBodies.Remove(Character);

		if (overlappingBodies.Count == 0)
		{
			ResetCombo();
			gameScene.EmitSignal(GameScene.SignalName.CharacterAttackMiss, Character);
		}

		foreach (var body in overlappingBodies)
		{
			if (body.HasNode("HealthComponent") && body.HasNode("AttackComponent"))
			{
				// Apply damage to the enemy
				var healthComponent = body.GetNode("HealthComponent") as HealthComponent;
				healthComponent.AddHealth(-CalculateDamage());

				// Apply knockback and paralyze effect
				ApplyKnockbackAndParalyze(body as CharacterBody2D, attackType);

				gameScene.EmitSignal(
					GameScene.SignalName.CharacterAttackFeedback,
					currentComboCount,
					CalculateDamage(),
					body
				);
			}
		}
	}

	// Apply knockback and paralyze to the enemy
	private void ApplyKnockbackAndParalyze(CharacterBody2D enemy, int attackType)
	{
		if (enemy == null)
			return;

		// Calculate knockback direction and magnitude
		Vector2 knockbackDirection = (enemy.GlobalPosition - GlobalPosition).Normalized();
		float knockbackStrength = (attackType + 1) * 100.0f; // Adjust the strength as needed

		Character.Velocity = knockbackDirection * knockbackStrength;

		enemy.Velocity = knockbackDirection * knockbackStrength;

		(enemy.GetNode("MovementComponent") as MovementComponent).Stun(AttackDurations[attackType]);
	}

	// Example method to calculate damage
	private int CalculateDamage()
	{
		// Basic damage calculation (could be based on combo count, weapon stats, etc.)
		return currentComboCount * 10;
	}
}
