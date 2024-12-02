using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VampireSurvivors;
using VampireSurvivors.Properties;

public class Fireball
{
    public Image Visual { get; private set; }
    public double Speed { get; private set; }
    public Vector Direction { get; set; }
    public int Damage { get; private set; }
    public double Radius { get; private set; }
    private Experience experience;
    private bool stopAnimation;

    private BitmapImage[] textures;
    private int currentTextureIndex = 0;
    private readonly DispatcherTimer animationTimer;

    public Fireball(Experience experience)
    {
        Speed = 7;
        Damage = 50;
        Radius = 25;
        this.experience = experience;
        textures = new BitmapImage[]
        {
                ImageHelper.LoadImageFromBytes(Resources.fireball1), 
                ImageHelper.LoadImageFromBytes(Resources.fireball2),
                ImageHelper.LoadImageFromBytes(Resources.fireball3),
                ImageHelper.LoadImageFromBytes(Resources.fireball4),
        };

        Visual = new Image
        {
            Width = Radius * 2,
            Height = Radius * 2,
            Source = textures[0],
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        animationTimer = new DispatcherTimer();
        animationTimer.Interval = TimeSpan.FromMilliseconds(50); 
        animationTimer.Tick += Animate;
        animationTimer.Start();
    }

    private void Animate(object sender, EventArgs e)
    {
        if (!stopAnimation) return;
        currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
        Visual.Source = textures[currentTextureIndex];
    }

    public void Move(Canvas canvas)
    {
        double x = Canvas.GetLeft(Visual) + Direction.X * Speed;
        double y = Canvas.GetTop(Visual) + Direction.Y * Speed;
        if (Direction.X != 0 || Direction.Y != 0)
        {
            double angle = Math.Atan2(Direction.Y, Direction.X) * (180 / Math.PI);
            Visual.RenderTransform = new RotateTransform(angle);
        }

        if (x < 0 || x > canvas.ActualWidth - Visual.Width)
        {
            Direction = new Vector(-Direction.X, Direction.Y);
        }
        if (y < 0 || y > canvas.ActualHeight - Visual.Height)
        {
            Direction = new Vector(Direction.X, -Direction.Y);
        }
        Canvas.SetLeft(Visual, x);
        Canvas.SetTop(Visual, y);
    }

    public void CheckCollisionWithEnemies(List<Enemy> enemies, Canvas canvas, TextBlock levelText)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            if (IsColliding(enemy.Visual))
            {
                enemy.TakeDamage(Damage);
                if (enemy.Health <= 0)
                {
                    experience.AddExperience(10, levelText);
                    enemies.RemoveAt(i);
                    canvas.Children.Remove(enemy.Visual);
                }
            }
        }
    }

    private bool IsColliding(UIElement enemyVisual)
    {
        double fireballCenterX = Canvas.GetLeft(Visual) + Radius;
        double fireballCenterY = Canvas.GetTop(Visual) + Radius;
        double enemyLeft = Canvas.GetLeft(enemyVisual);
        double enemyTop = Canvas.GetTop(enemyVisual);
        double enemyWidth = 40;
        double enemyHeight = 50;
        double nearestX = Math.Max(enemyLeft, Math.Min(fireballCenterX, enemyLeft + enemyWidth));
        double nearestY = Math.Max(enemyTop, Math.Min(fireballCenterY, enemyTop + enemyHeight));
        double deltaX = fireballCenterX - nearestX;
        double deltaY = fireballCenterY - nearestY;
        double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        return distance <= Radius;
    }

    public void Upgrade()
    {
        Radius += 5;
        Visual.Width = Radius * 2;
        Visual.Height = Radius * 2;
        Speed += 1;
        Damage += 10;
    }

    public void StartAnimation()
    {
        stopAnimation = true;
    }

    public void StopAnimation()
    {
        stopAnimation = false;
    }
}