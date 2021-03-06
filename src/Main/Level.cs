using Godot;
using System;

public class Level : Node
{
    [Export]
    public string levelName;
    private HUD _hud;
    private Player _player;
    private PackedScene _gameOverScreen;

    public override void _Ready()
    {
        _player = GetNode<Player>("Player");
        ConnectPlayerSignals();

        var hudScene = ResourceLoader.Load("res://src/UserInterface/HUD.tscn") as PackedScene;
        _hud = hudScene.Instance() as HUD;
        AddChild(_hud);
        _hud.UpdateLives(_player.Health);

        _gameOverScreen = ResourceLoader.Load("res://src/UserInterface/GameOverScreen.tscn") as PackedScene;
    }

    private void ConnectPlayerSignals()
    {
        _player.Connect("UpdateStats", this, nameof(OnPlayerUpdateStats));
        _player.Connect("EndGame", this, nameof(OnPlayerEndGame));
    }

    public void OnPlayerUpdateStats(int lives)
    {
        Console.WriteLine("update stats");
        _hud.UpdateLives(lives);
    }

    public void OnPlayerEndGame()
    {
        var gameOver = _gameOverScreen.Instance() as GameOverScreen;
        AddChild(gameOver);

        gameOver.Connect("PlayAgain", this, nameof(Restart));
    }

    private void Restart()
    {
        GetTree().ChangeScene($"res://src/Main/{levelName}.tscn");
    }
}
