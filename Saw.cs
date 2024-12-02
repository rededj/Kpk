using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VampireSurvivors.Properties;

namespace VampireSurvivors
{
    public class Saw
    {
        public int Damage { get; set; } = 15;
        public int NumberOfSaws { get; set; } = 1;
        public double RotationSpeed { get; set; } = 6;
        private readonly DispatcherTimer rotationTimer;
        public List<Image> sawVisuals = new List<Image>();
        private double angle = 0;
        private const double distanceFromPlayer = 100;
        private double playerX;
        private double playerY;
        private Experience experience;
        private bool isMoving = true;

        private BitmapImage[] textures;
        private int currentTextureIndex = 0;
        private readonly DispatcherTimer animationTimer;

        public Saw(double initialPlayerX, double initialPlayerY, Experience experience)
        {
            playerX = initialPlayerX;
            playerY = initialPlayerY;
            this.experience = experience;
            textures = new BitmapImage[]
            {
                ImageHelper.LoadImageFromBytes(Resources.saw1),
                ImageHelper.LoadImageFromBytes(Resources.saw2),
                ImageHelper.LoadImageFromBytes(Resources.saw3),
            };

            rotationTimer = new DispatcherTimer();
            rotationTimer.Interval = TimeSpan.FromMilliseconds(20);
            rotationTimer.Tick += RotateSaws;
            rotationTimer.Start();
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100); 
            animationTimer.Tick += AnimateSaws;
            animationTimer.Start();
        }

        public void CreateSaws(Canvas gameCanvas)
        {
            double angleIncrement = 360.0 / NumberOfSaws;

            for (int i = 0; i < NumberOfSaws; i++)
            {
                double currentAngle = i * angleIncrement;
                Image saw = CreateSawVisual();
                sawVisuals.Add(saw);
                gameCanvas.Children.Add(saw);
                UpdateSawPosition(saw, currentAngle);
            }
        }

        public Image CreateSawVisual()
        {
            var saw = new Image
            {
                Width = 30,
                Height = 30,
                Source = textures[0] 
            };
            return saw;
        }

        private void RotateSaws(object sender, EventArgs e)
        {
            if (!isMoving) return;

            angle += RotationSpeed;
            if (angle >= 360) angle -= 360;

            for (int i = 0; i < sawVisuals.Count; i++)
            {
                double sawAngle = angle + (360.0 / NumberOfSaws) * i;
                UpdateSawPosition(sawVisuals[i], sawAngle);
            }
        }

        private void AnimateSaws(object sender, EventArgs e)
        {
            if (!isMoving) return;
            currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
            foreach (var saw in sawVisuals)
            {
                saw.Source = textures[currentTextureIndex];
            }
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
            foreach (var saw in sawVisuals)
            {
                double sawCenterX = Canvas.GetLeft(saw) + 10;
                double sawCenterY = Canvas.GetTop(saw) + 10;
                double enemyLeft = Canvas.GetLeft(enemyVisual);
                double enemyTop = Canvas.GetTop(enemyVisual);
                double enemyWidth = 40;
                double enemyHeight = 50;
                double nearestX = Math.Max(enemyLeft, Math.Min(sawCenterX, enemyLeft + enemyWidth));
                double nearestY = Math.Max(enemyTop, Math.Min(sawCenterY, enemyTop + enemyHeight));
                double deltaX = sawCenterX - nearestX;
                double deltaY = sawCenterY - nearestY;
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                if (distance <= 10)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateSawPosition(UIElement saw, double angle)
        {
            double radians = angle * Math.PI / 180;
            double sawX = playerX + distanceFromPlayer * Math.Cos(radians) + 9;
            double sawY = playerY + distanceFromPlayer * Math.Sin(radians) + 15;

            Canvas.SetLeft(saw, sawX);
            Canvas.SetTop(saw, sawY);
        }

        public void UpdatePlayerPosition(double newPlayerX, double newPlayerY)
        {
            playerX = newPlayerX;
            playerY = newPlayerY;
        }

        public void Upgrade(Canvas gameCanvas)
        {
            NumberOfSaws += 1;
            AddNewSaw(gameCanvas);
            Damage += 10;
        }

        private void AddNewSaw(Canvas gameCanvas)
        {
            double angleIncrement = 360.0 / NumberOfSaws;
            double currentAngle = (NumberOfSaws - 1) * angleIncrement;
            Image saw = CreateSawVisual();
            sawVisuals.Add(saw);
            gameCanvas.Children.Add(saw);
            UpdateSawPosition(saw, currentAngle);
        }

        public void StartMovement()
        {
            isMoving = true;
        }

        public void StopMovement()
        {
            isMoving = false;
        }

        public void RemoveSaws(Canvas gameCanvas)
        {
            foreach (var saw in sawVisuals)
            {
                gameCanvas.Children.Remove(saw);
            }
            sawVisuals.Clear();
        }
    }
}