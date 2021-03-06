using Godot;
using System;

public class Enemy : KinematicBody2D
{
    const int GRAVITY = 20;
    const int SPEED = 125;
    const float DEATH_TIME = 1.0f;
    public Vector2 UP = Vector2.Up; // const

    public int Health = 2;
    public Vector2 Direction = Vector2.Left;
    private bool _isDead = false;
    private Vector2 _motion;
    private AnimatedSprite _sprite;
    private RayCast2D _rayCast;
    private Area2D _top;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite>("AnimatedSprite");
        _rayCast = GetNode<RayCast2D>("RayCast2D");
        _top = GetNode<Area2D>("Area2D");
    }

    public override void _PhysicsProcess(float delta)
    {
        // Don't move if dead
        if (_isDead)
        {
            return;
        }

        _sprite.Play("walk");
        _motion.x = SPEED * Direction.x;
        _motion.y += GRAVITY;
        _motion = this.MoveAndSlide(_motion, UP);

        if (this.IsOnWall() || !_rayCast.IsColliding()) {
            FlipHDirection();
        }
        ProcessCollisions();
    }

    // Flip RayCast and sprite when enemy changes directions
    private void FlipHDirection()
    {
        _sprite.FlipH = !_sprite.FlipH;
        Direction.x *= -1;
        _rayCast.Position = new Vector2(-1 *_rayCast.Position.x + 2.5f, _rayCast.Position.y);
    }

    // If enemy collides with player, hit player
    private void ProcessCollisions()
    {
        for (int i = 0; i < GetSlideCount(); i++)
        {
            var collision = GetSlideCollision(i);
            if (collision.Collider is Player player)
            {
                player.Hit();
            }
        }

        var bodies = _top.GetOverlappingBodies();
        foreach (Node body in bodies)
        {
            if (body is Player player)
            {
                player.Hit();
            }
        }
    }

    public void Hit()
    {
        Console.WriteLine("hit enemy");
        Health--;
        if (Health == 0)
        {
            Fade();
        }
    }

    private void Fade()
    {
        _isDead = true;
        _sprite.Play("death");
        
        // Fade out enemy
        var tween = GetNode<Tween>("Tween");
        var startColor = new Color(1.0f, 1.0f, 1.0f);
        var endColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        tween.InterpolateProperty(
            this, "modulate", startColor, endColor, DEATH_TIME, Tween.TransitionType.Linear, Tween.EaseType.In
        );
        tween.Start();

        // Remove collision
        var collision = GetNode<CollisionShape2D>("CollisionShape2D");
        RemoveChild(collision);
        collision.QueueFree();

        // Remove enemy after tween completes
        var timer = new Timer();
        timer.OneShot = true;
        timer.Connect("timeout", this, nameof(Die));
        AddChild(timer);
        timer.Start(DEATH_TIME);
    }

    private void Die()
    {
        QueueFree();
    }
}
