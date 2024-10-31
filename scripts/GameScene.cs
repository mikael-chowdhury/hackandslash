using Godot;
using System;
using System.Diagnostics;

public partial class GameScene : Node2D
{
	[Signal]
	public delegate void CharacterAttackFeedbackEventHandler(
		int comboNumber,
		float damage,
		CharacterBody2D hit
	);

	[Signal]
	public delegate void CharacterAttackMissEventHandler(CharacterBody2D character);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Maximized);

		CharacterAttackFeedback += CharacterAttackFeedbackHandler;
		CharacterAttackMiss += CharacterAttackMissHandler;
	}

	public void CharacterAttackFeedbackHandler(int comboNumber, float damage, CharacterBody2D hit)
	{
		DisplayShortMessage(
			comboNumber.ToString(),
			new Vector2(1, 1),
			hit.Position + new Vector2(0, -200),
			0.75f,
			new Color(1, 1, 1, 1)
		);
	}

	public void CharacterAttackMissHandler(CharacterBody2D character)
	{
		DisplayShortMessage(
			"Miss",
			new Vector2(1, 1),
			character.Position + new Vector2(0, -200),
			0.75f,
			new Color(1, 1, 1, 1)
		);
	}

	private void DisplayShortMessage(
		string message,
		Vector2 Scale,
		Vector2 Position,
		float durationSeconds,
		Color color
	)
	{
		// Create the label
		Label comboLabel =
			new()
			{
				Text = message,
				Modulate = color,
				Scale = Scale,
				RotationDegrees = (float)new Random().NextDouble() * 30 - 15,
				Position = Position + new Vector2((float)new Random().NextDouble() * 200 - 100, 0),
				ZIndex = 100,
			};

		comboLabel.Set("theme_override_font_sizes/font_size", 128);

		AddChild(comboLabel);

		SceneTreeTimer timer = GetTree().CreateTimer(durationSeconds);
		timer.Connect("timeout", Callable.From(() => RemoveChild(comboLabel)));
	}
}
