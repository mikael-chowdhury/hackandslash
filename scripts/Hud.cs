using Godot;
using System;

public partial class Hud : Control
{
    [Signal]
    public delegate void PlayerStunnedEventHandler(Player PlayerCharacter, float durationSeconds);

    public CharacterBody2D Character { get; set; }
    public AttackComponent CharacterAttackComponent { get; set; }

    private Control _stunContainer;
    private Label _stunLabel;
    private TextureProgressBar _stunProgress;
    private float _stunDuration;
    private float _stunTimeElapsed;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node _gameScene = GetTree().Root.GetNode("RootApp").GetChild(0);

        Character = _gameScene.GetChild(0) as CharacterBody2D;
        CharacterAttackComponent = Character.GetNode<AttackComponent>("AttackComponent");

        _gameScene.Connect(
            GameScene.SignalName.CharacterAttackFeedback,
            new Callable(this, MethodName.CharacterAttackFeedbackHandle)
        );

        PlayerStunned += PlayerStunnedHandle;

        // Create and set up stun UI elements
        _stunContainer = new Control();
        _stunContainer.AnchorLeft = 0.0f;
        _stunContainer.AnchorTop = 0.0f;
        _stunContainer.AnchorRight = 1.0f;
        _stunContainer.AnchorBottom = 0.1f;
        _stunContainer.Visible = false;
        AddChild(_stunContainer);

        _stunLabel = new Label();
        _stunLabel.Text = "STUNNED";
        _stunLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _stunLabel.VerticalAlignment = VerticalAlignment.Center;
        _stunLabel.AnchorLeft = 0.0f;
        _stunLabel.AnchorTop = 0.0f;
        _stunLabel.AnchorRight = 1.0f;
        _stunLabel.AnchorBottom = 1.0f;
        _stunLabel.Set("theme_override_font_sizes/font_size", 156);

        _stunContainer.AddChild(_stunLabel);

        _stunProgress = new TextureProgressBar();
        _stunProgress.AnchorLeft = 0.25f;
        _stunProgress.AnchorTop = 0.25f;
        _stunProgress.AnchorRight = 0.75f;
        _stunProgress.AnchorBottom = 0.75f;
        _stunProgress.MinValue = 0.0f;
        _stunProgress.MaxValue = 100.0f;
        _stunProgress.Value = 100.0f;
        // Set your circular progress textures here
        // _stunProgress.UnderTexture = (Texture)GD.Load("res://path_to_under_texture.png");
        _stunProgress.TextureProgress = (Texture)GD.Load("res://assets/loading.png") as Texture2D;
        _stunContainer.AddChild(_stunProgress);
    }

    public void PlayerStunnedHandle(Player Player, float durationSeconds)
    {
        GD.Print("STUNNED");
        _stunDuration = durationSeconds;
        _stunTimeElapsed = 0.0f;
        _stunContainer.Visible = true;
        _stunProgress.MaxValue = durationSeconds;
        _stunProgress.Value = durationSeconds;
    }

    public void CharacterAttackFeedbackHandle(int comboNumber, float damage, CharacterBody2D hit)
    {
        // Handle character attack feedback
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_stunContainer.Visible)
        {
            _stunTimeElapsed += (float)delta;
            _stunProgress.Value = _stunDuration - _stunTimeElapsed;

            if (_stunTimeElapsed >= _stunDuration)
            {
                _stunContainer.Visible = false;
            }
        }
    }
}
