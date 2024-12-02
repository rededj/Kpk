using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;

namespace VampireSurvivors
{
    public class Enemy
    {
        private static double baseSpeed = 3;
        private static int baseHealth = 200;
        private static int baseDamage = 5;

        public Image Visual { get; private set; }
        public double Speed { get; private set; }
        public int Health { get; private set; }
        public int Damage { get; private set; }

        private BitmapImage ImagePath1 => ImageHelper.LoadImageFromBytes(Properties.Resources.Knight1);
        private BitmapImage ImagePath2 => ImageHelper.LoadImageFromBytes(Properties.Resources.Knight2);
        private BitmapImage ImagePath3 => ImageHelper.LoadImageFromBytes(Properties.Resources.Knight3);
        private BitmapImage ImagePath4 => ImageHelper.LoadImageFromBytes(Properties.Resources.Knight4);

        private BitmapImage[] imagePaths;
        private DispatcherTimer animationTimer;
        private int currentFrame;

        public Enemy(double startX, double startY)
        {
            Speed = baseSpeed;
            Health = baseHealth;
            Damage = baseDamage;

            imagePaths = new BitmapImage[] { ImagePath1, ImagePath2, ImagePath3, ImagePath4 };
            Visual = new Image
            {
                Source = imagePaths[0],
                Width = 40,
                Height = 50,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };
            Canvas.SetLeft(Visual, startX);
            Canvas.SetTop(Visual, startY);

            StartAnimation();
        }

        public void StartAnimation()
        {
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            animationTimer.Tick += OnAnimationTick;
            animationTimer.Start();
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            currentFrame = (currentFrame + 1) % imagePaths.Length;
            Visual.Source = imagePaths[currentFrame];
        }

        public void StopAnimation()
        {
            animationTimer?.Stop();
        }

        public void MoveTowards(double playerX, double playerY, List<Enemy> enemies)
        {
            double enemyX = Canvas.GetLeft(Visual);
            double enemyY = Canvas.GetTop(Visual);

            double directionX = playerX - enemyX;
            double directionY = playerY - enemyY;
            double length = Math.Sqrt(directionX * directionX + directionY * directionY);

            if (length > 0)
            {
                directionX /= length;
                directionY /= length;

                double newX = enemyX + directionX * Speed;
                double newY = enemyY + directionY * Speed;

                if (directionX > 0)
                {
                    ((ScaleTransform)Visual.RenderTransform).ScaleX = 1;
                }
                else if (directionX < 0)
                {
                    ((ScaleTransform)Visual.RenderTransform).ScaleX = -1;
                }

                Canvas.SetLeft(Visual, newX);
                Canvas.SetTop(Visual, newY);
            }
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Die();
            }
        }

        public static void Upgrade()
        {
            baseHealth += 200;
            baseDamage += 1;
            baseSpeed = Math.Max(baseSpeed + 0.5, 4); 
        }

        public static void ResetUpgrades()
        {
            baseSpeed = 3; 
            baseHealth = 200; 
            baseDamage = 5; 
        }

        private void Die()
        {
            StopAnimation();
            Visual.Visibility = Visibility.Collapsed;
        }
    }
}