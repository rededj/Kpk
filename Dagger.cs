using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Generic;
using VampireSurvivors;
using VampireSurvivors.Properties;

public class Dagger
{
    public int Damage { get; set; }
    public double AttackSpeed { get; set; }
    public double DistanceToTravel { get; set; }
    private DispatcherTimer attackTimer;
    private Vector attackDirection;
    private Game game;

    public Dagger()
    {
        Damage = 50;
        AttackSpeed = 1000;
        DistanceToTravel = 200;
        attackTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(AttackSpeed)
        };
        attackTimer.Tick += (s, e) => ExecuteAttack();
    }

    public void SetAttackDirection(Vector direction)
    {
        attackDirection = direction;
    }

    public void StartAutoAttack(Game gameInstance)
    {
        game = gameInstance;
        attackTimer.Start();
    }

    public void StopAutoAttack()
    {
        attackTimer.Stop();
    }

    private void ExecuteAttack()
    {
        if (game.IsPaused || game.PlayerHealth <= 0) return;

        double startX = Canvas.GetLeft(game.Player);
        double startY = Canvas.GetTop(game.Player) + 10;

        if (attackDirection == default(Vector))
        {
            attackDirection = new Vector(1, 0);
        }

        startX += attackDirection.X < 0 ? -15 : (attackDirection.X > 0 ? 15 : 0);
        startY += attackDirection.Y < 0 ? -15 : (attackDirection.Y > 0 ? 15 : 0);
        var projectile = new Projectile(startX, startY, attackDirection, Damage, DistanceToTravel);
        game.Projectiles.Add(projectile);
        game.GameCanvas.Children.Add(projectile.Visual);
    }

    public class Projectile
    {
        public Image Visual { get; private set; }
        public double Speed { get; set; } = 10;
        public Vector Direction { get; private set; }
        private double distanceTravelled;
        public int Damage { get; private set; }
        public Rect HitBox { get; private set; }
        public double DistanceToTravel { get; private set; }

        public Projectile(double startX, double startY, Vector direction, int damage, double distanceToTravel)
        {
            Visual = new Image
            {
                Source = ImageHelper.LoadImageFromBytes(Resources.dagger),
                Width = 50,
                Height = 40
            };

            Canvas.SetLeft(Visual, startX);
            Canvas.SetTop(Visual, startY);
            Direction = direction;
            Damage = damage;
            UpdateHitBox();
            RotateArrow();
            DistanceToTravel = distanceToTravel;
        }

        private void UpdateHitBox()
        {
            double width = 25;
            double height = 20;
            double posX = Canvas.GetLeft(Visual);
            double posY = Canvas.GetTop(Visual);
            HitBox = new Rect(posX, posY, width, height);
        }

        private void RotateArrow()
        {
            double angle = Math.Atan2(Direction.Y, Direction.X) * (180 / Math.PI);
            Visual.RenderTransform = new RotateTransform(angle);
            Visual.RenderTransformOrigin = new Point(0.5, 0.5); 
        }

        public void Move()
        {
            double newLeft = Canvas.GetLeft(Visual) + Direction.X * Speed;
            double newTop = Canvas.GetTop(Visual) + Direction.Y * Speed;
            Canvas.SetLeft(Visual, newLeft);
            Canvas.SetTop(Visual, newTop);
            distanceTravelled += Speed;
            UpdateHitBox();
            if (distanceTravelled >= DistanceToTravel)
            {
                var parent = Visual.Parent as Canvas;
                parent?.Children.Remove(Visual);
            }
        }
    }
}