using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;

namespace VampireSurvivors
{
    public partial class Game : Window
    {
        public int playerHealth = 1000;
        private double playerSpeed = 6;
        private double playerHPRegen = 0;
        private DispatcherTimer gameTimer;
        private double deltaX = 0;
        private double deltaY = 0;
        private bool isPaused = false;
        public List<Enemy> enemies = new List<Enemy>();
        private TimeSpan gameTime;
        private TimeSpan lastSpawnTime;
        private TimeSpan lastUpTime;
        private TimeSpan lastEnemyUpgradeTime;
        private double enemyMultiplier = 1.0;
        private const int baseEnemyCount = 20;
        private Dagger playerWeapon;
        private Fireball fireball;
        private Saw saw;
        private List<Dagger.Projectile> projectiles = new List<Dagger.Projectile>();
        private Experience playerExperience;
        private PauseMenu pauseMenu;
        private BitmapImage IdleImage => ImageHelper.LoadImageFromBytes(Properties.Resources.main);
        private BitmapImage WalkImage1 => ImageHelper.LoadImageFromBytes(Properties.Resources.main1);
        private BitmapImage WalkImage2 => ImageHelper.LoadImageFromBytes(Properties.Resources.main2);
        private BitmapImage WalkImage3 => ImageHelper.LoadImageFromBytes(Properties.Resources.main3);
        private BitmapImage WalkImage4 => ImageHelper.LoadImageFromBytes(Properties.Resources.main4);
        private BitmapImage[] playerImagePaths;
        private List<UpgradeOption> upgradeOptions = new List<UpgradeOption>
        {
            new UpgradeOption("Увеличить скорость движения"),
            new UpgradeOption("Увеличить регенерацию здоровья"),
            new UpgradeOption("Улучшить кинжал"),
            new UpgradeOption("Улучшить фаербол"),
            new UpgradeOption("Улучшить пилы")
        };
        private DispatcherTimer playerAnimationTimer;
        private DispatcherTimer healthRegenTimer;
        private int currentPlayerFrame = 1;
        private bool isMoving = false;
        private const int TotalFrames = 4;
        Random random = new Random();
        public bool IsPaused => isPaused;
        public int PlayerHealth => playerHealth;
        public List<Dagger.Projectile> Projectiles => projectiles;

        public Game(bool fullScreen, byte[] backgroundImagePath)
        {
            InitializeComponent();
            playerExperience = new Experience(ExperienceBar);
            playerExperience.LevelUp += ShowLevelUpMenu;
            pauseMenu = new PauseMenu(this);
            this.Loaded += Game_Loaded;
            playerWeapon = new Dagger();
            playerWeapon.StartAutoAttack(this);
            pauseMenu.ChangeBackground(backgroundImagePath);
            playerAnimationTimer = new DispatcherTimer();
            playerAnimationTimer.Interval = TimeSpan.FromMilliseconds(100);
            playerAnimationTimer.Tick += PlayerAnimationTimer_Tick;
            healthRegenTimer = new DispatcherTimer();
            healthRegenTimer.Interval = TimeSpan.FromSeconds(1);
            healthRegenTimer.Tick += HealthRegenTimer_Tick;
            playerImagePaths = new BitmapImage[] { WalkImage1, WalkImage2, WalkImage3, WalkImage4, IdleImage};
            Cursor = Cursors.None;
            if (fullScreen)
            {
                GoFullScreen();
            }
            else
            {
                GoMaximisedScreen();
            }
        }

        private void PlayerAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (isMoving)
            {
                currentPlayerFrame = (currentPlayerFrame + 1) % TotalFrames;
                Player.Source = isMoving ? GetCurrentWalkImage() : IdleImage;
            }
        }

        private BitmapImage GetCurrentWalkImage()
        {
            return ImageHelper.GetCurrentWalkImage(currentPlayerFrame, WalkImage1, WalkImage2, WalkImage3, WalkImage4, IdleImage);
        }

        private void Game_Loaded(object sender, RoutedEventArgs e)
        {
            StartGame();
            CreateEnemies();
            FullScreenToggle.IsChecked = (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None);
        }

        public void GoFullScreen()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
        }

        public void GoMaximisedScreen()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized;
        }

        private void StartGame()
        {
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
            gameTime = TimeSpan.Zero;
            lastSpawnTime = TimeSpan.Zero;
            lastUpTime = TimeSpan.Zero;
            enemyMultiplier = 1.0;
            healthRegenTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            MovePlayer(deltaX, deltaY);
            gameTime = gameTime.Add(TimeSpan.FromMilliseconds(20));
            TimerText.Text = string.Format("{0:D2}:{1:D2}", (int)gameTime.TotalMinutes, gameTime.Seconds);
            fireball?.Move(GameCanvas);
            fireball?.CheckCollisionWithEnemies(enemies, GameCanvas, LevelText);
            saw?.CheckCollisionWithEnemies(enemies, GameCanvas, LevelText);
            if (gameTime.TotalSeconds - lastSpawnTime.TotalSeconds >= 8)
            {
                CreateEnemies();
                lastSpawnTime = gameTime;
            }
            if ((int)gameTime.TotalSeconds - lastUpTime.TotalSeconds == 15)
            {
                enemyMultiplier += 0.1;
                lastUpTime = gameTime;
            }
            if (gameTime.TotalSeconds - lastEnemyUpgradeTime.TotalSeconds >= 60)
            {
                Enemy.Upgrade(); 
                lastEnemyUpgradeTime = gameTime; 
            }
        }

        private void CreateEnemies()
        {
            int currentEnemyCount = (int)(baseEnemyCount + (baseEnemyCount * (enemyMultiplier - 1)));
            double centerX = GameCanvas.ActualWidth / 2;
            double centerY = GameCanvas.ActualHeight / 2;
            const int widthMultiply = 2;
            const int heightMultiply = 1;

            for (int i = 0; i < currentEnemyCount; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double distance = 500 + random.NextDouble() * 100;
                double startX = centerX + (distance * widthMultiply) * Math.Cos(angle);
                double startY = centerY + (distance * heightMultiply) * Math.Sin(angle);
                Enemy enemy = new Enemy(startX, startY);
                enemy.Visual.SetValue(Canvas.ZIndexProperty, 0);
                enemies.Add(enemy);
                GameCanvas.Children.Add(enemy.Visual);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (isPaused) return;
            isMoving = true;
            switch (e.Key)
            {
                case Key.A:
                    deltaX = -playerSpeed;
                    Player.RenderTransform = new ScaleTransform(-1, 1);
                    playerWeapon.SetAttackDirection(new Vector(-1, 0));
                    break;
                case Key.D:
                    deltaX = playerSpeed;
                    Player.RenderTransform = new ScaleTransform(1, 1);
                    playerWeapon.SetAttackDirection(new Vector(1, 0));
                    break;
                case Key.W:
                    deltaY = -playerSpeed;
                    playerWeapon.SetAttackDirection(new Vector(0, -1));
                    break;
                case Key.S:
                    deltaY = playerSpeed;
                    playerWeapon.SetAttackDirection(new Vector(0, 1));
                    break;
            }
            if (!playerAnimationTimer.IsEnabled)
                playerAnimationTimer.Start();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (isPaused || Lose.Visibility == Visibility.Visible)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.A:
                case Key.D:
                    deltaX = 0;
                    break;
                case Key.W:
                case Key.S:
                    deltaY = 0;
                    break;
                case Key.Escape:
                    if (isPaused)
                    {
                        Pause();
                        Menu.Visibility = Visibility.Collapsed;
                        PauseMenu.Visibility = Visibility.Collapsed;
                        Settings.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Menu.Visibility = Visibility.Visible;
                        PauseMenu.Visibility = Visibility.Visible;
                        Pause();
                        deltaX = 0;
                        deltaY = 0;
                        playerAnimationTimer.Stop(); 
                        currentPlayerFrame = 0;
                        Player.Source = isMoving ? GetCurrentWalkImage() : IdleImage;
                    }
                    break;
            }
            if (deltaX == 0 && deltaY == 0)
            {
                playerAnimationTimer.Stop(); 
                currentPlayerFrame = 0;
                Player.Source = playerImagePaths[4];
            }
        }

        private void MovePlayer(double deltaX, double deltaY)
        {
            double newLeft = Canvas.GetLeft(Player) + deltaX;
            double newTop = Canvas.GetTop(Player) + deltaY;

            if (newLeft >= 0 && newLeft <= GameCanvas.ActualWidth - Player.Width)
                Canvas.SetLeft(Player, newLeft);
            if (newTop >= 0 && newTop <= GameCanvas.ActualHeight - Player.Height)
                Canvas.SetTop(Player, newTop);

            double playerX = Canvas.GetLeft(Player);
            double playerY = Canvas.GetTop(Player);
            if (saw != null)
            {
                saw.UpdatePlayerPosition(playerX, playerY); 
            }

            if (deltaX != 0 || deltaY != 0)
            {
                Vector direction = new Vector(deltaX, deltaY);
                direction.Normalize();
                playerWeapon.SetAttackDirection(direction);
            }

            playerX = Canvas.GetLeft(Player);
            playerY = Canvas.GetTop(Player);

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];

                if (enemy.Health > 0)
                {
                    enemy.MoveTowards(playerX, playerY, enemies);

                    if (IsColliding(Player, enemy.Visual))
                    {
                        TakeDamage(enemy.Damage);
                        enemy.TakeDamage(5);
                        if (enemy.Health <= 0)
                        {
                            GameCanvas.Children.Remove(enemy.Visual);
                            enemies.RemoveAt(i);
                            playerExperience.AddExperience(10, LevelText);
                        }
                    }
                }
            }
            foreach (var projectile in projectiles.ToArray())
            {
                projectile.Move();

                if (Canvas.GetLeft(projectile.Visual) < 0 || Canvas.GetLeft(projectile.Visual) > GameCanvas.ActualWidth ||
                    Canvas.GetTop(projectile.Visual) < 0 || Canvas.GetTop(projectile.Visual) > GameCanvas.ActualHeight)
                {
                    GameCanvas.Children.Remove(projectile.Visual);
                    projectiles.Remove(projectile);
                }
                else
                {
                    HandleProjectileCollision(projectile);
                }
            }
        }

        private void HandleProjectileCollision(Dagger.Projectile projectile)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];

                if (IsColliding(projectile.Visual, enemy.Visual))
                {
                    enemy.TakeDamage(projectile.Damage);
                    if (enemy.Health <= 0)
                    {
                        GameCanvas.Children.Remove(enemy.Visual);
                        enemies.RemoveAt(i);
                        playerExperience.AddExperience(10, LevelText);
                    }
                    break;
                }
            }
        }

        public bool IsColliding(UIElement element1, UIElement element2)
        {
            Rect bounds1 = VisualTreeHelper.GetDescendantBounds(element1);
            Rect bounds2 = VisualTreeHelper.GetDescendantBounds(element2);

            Point position1 = element1.TranslatePoint(new Point(0, 0), GameCanvas);
            Point position2 = element2.TranslatePoint(new Point(0, 0), GameCanvas);

            bounds1.X += position1.X;
            bounds1.Y += position1.Y;
            bounds2.X += position2.X;
            bounds2.Y += position2.Y;

            return bounds1.IntersectsWith(bounds2);
        }

        private void TakeDamage(int damage)
        {
            if (playerHealth <= 0) return;
            playerHealth -= damage;
            UpdateHPBar();

            if (playerHealth <= 0)
            {
                Pause();
                Lose.Visibility = Visibility.Visible;
            }
        }

        private void UpdateHPBar()
        {
            double maxHealth = 1000;
            double healthPercentage = playerHealth / maxHealth;
            if (healthPercentage <= 0)
            {
                HPBar.Width = 0;
                return;
            }
            HPBar.Width = 300 * healthPercentage;
            Canvas.SetLeft(HPBarBackground, 1165);
            Canvas.SetTop(HPBarBackground, 10);
            Canvas.SetLeft(HPBar, 1165);
            Canvas.SetTop(HPBar, 10);
        }

        private void HealthRegenTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused || playerHealth >= 1000 || playerHPRegen == 0)
            {
                return; 
            }
            playerHealth += (int)playerHPRegen;
            if (playerHealth > 1000)
            {
                playerHealth = 1000;
            }
            UpdateHPBar();
        }

        private List<string> GenerateRandomUpgrades()
        {
            var availableUpgrades = upgradeOptions.Where(u => u.CanBeSelected()).ToList();

            return availableUpgrades.OrderBy(x => random.Next())
                                     .Take(Math.Min(3, availableUpgrades.Count))
                                     .Select(u => u.Name)
                                     .ToList();
        }

        private void OnUpgradeSelected(object sender, RoutedEventArgs e)
        {
            Button selectedButton = sender as Button;
            string selectedUpgrade = selectedButton.Content.ToString();
            ApplyUpgrade(selectedUpgrade);
            var upgrade = upgradeOptions.FirstOrDefault(u => u.Name == selectedUpgrade);
            if (upgrade != null)
            {
                upgrade.Count++;
            }
            LevelUpMenu.Visibility = Visibility.Collapsed;
            isPaused = true;
            Pause();
        }

        private void ShowLevelUpMenu()
        {
            if (playerExperience.Level >= playerExperience.maxLevel)
            {
                playerHealth += 200;
                if (playerHealth > 1000) 
                {
                    playerHealth = 1000; 
                }
                UpdateHPBar();
                return;
            }
            isPaused = false;
            Pause();
            var upgrades = GenerateRandomUpgrades();
            UpgradeButton1.Content = upgrades.ElementAtOrDefault(0);
            UpgradeButton2.Content = upgrades.ElementAtOrDefault(1);
            UpgradeButton3.Content = upgrades.ElementAtOrDefault(2);
            UpgradeButton1.Visibility = upgrades.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            UpgradeButton2.Visibility = upgrades.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            UpgradeButton3.Visibility = upgrades.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
            LevelUpMenu.Visibility = Visibility.Visible;
        }

        private void ApplyUpgrade(string upgrade)
        {
            double playerX;
            double playerY;
            switch (upgrade)
            {
                case "Увеличить скорость движения":
                    playerSpeed += 0.4;
                    break;
                case "Увеличить регенерацию здоровья":
                    playerHPRegen += 4;
                    break;
                case "Улучшить кинжал":
                    playerWeapon.DistanceToTravel += 20;
                    playerWeapon.AttackSpeed -= 20;
                    playerWeapon.Damage += 20;
                    break;
                case "Улучшить фаербол":
                    if (fireball == null)
                    {
                        fireball = new Fireball(playerExperience);
                        playerX = Canvas.GetLeft(Player) + Player.Width / 2;
                        playerY = Canvas.GetTop(Player) + Player.Height / 2;
                        Canvas.SetLeft(fireball.Visual, playerX);
                        Canvas.SetTop(fireball.Visual, playerY);
                        fireball.Direction = new Vector(1, -1);
                        GameCanvas.Children.Add(fireball.Visual);
                    }
                    else
                    {
                        fireball.Upgrade();
                    }
                    break;
                case "Улучшить пилы":
                    if (saw == null)
                    { 
                        playerX = Canvas.GetLeft(Player) + 9;
                        playerY = Canvas.GetTop(Player) + 15;
                        saw = new Saw(playerX, playerY, playerExperience);
                        saw.CreateSaws(GameCanvas);
                    }
                    else
                    {
                        saw.Upgrade(GameCanvas);
                    }
                    break;
            }
        }

        private void PlayAgain(object sender, RoutedEventArgs e)
        {
            playerHealth = 1000;
            playerHPRegen = 0;
            playerSpeed = 6;
            Enemy.ResetUpgrades();
            playerExperience.Reset(LevelText);
            isPaused = false;
            Lose.Visibility = Visibility.Collapsed;
            foreach (var upgrade in upgradeOptions)
            {
                upgrade.Count = 0; 
            }
            foreach (var enemy in enemies)
            {
                GameCanvas.Children.Remove(enemy.Visual);
            }
            enemies.Clear();
            foreach (var projectile in projectiles.ToArray())
            {
                GameCanvas.Children.Remove(projectile.Visual);
            }
            projectiles.Clear();
            if (fireball != null)
            {
                GameCanvas.Children.Remove(fireball.Visual);
                fireball = null; 
            }
            if (saw != null)
            {
                saw.RemoveSaws(GameCanvas);
                saw = null; 
            }
            double centerX = (GameCanvas.ActualWidth - Player.Width) / 2;
            double centerY = (GameCanvas.ActualHeight - Player.Height) / 2;
            Canvas.SetLeft(Player, centerX);
            Canvas.SetTop(Player, centerY);
            deltaX = 0;
            deltaY = 0;

            StartGame();
            CreateEnemies();
            UpdateHPBar();
        }

        private void LeaveFromLose(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void Pause()
        {
            if (isPaused)
            {
                gameTimer.Start();
                foreach (var enemy in enemies)
                { 
                    enemy.StartAnimation();
                }
                HideCursor();
                if (saw != null)
                {
                    saw.StartMovement();
                }
                if (fireball != null)
                {
                    fireball.StartAnimation();
                }
            }
            else
            {
                gameTimer.Stop();
                foreach (var enemy in enemies)
                {
                    enemy.StopAnimation(); 
                } 
                ShowCursor();
                deltaX = 0; 
                deltaY = 0;
                playerAnimationTimer.Stop();
                currentPlayerFrame = 0; 
                Player.Source = playerImagePaths[4];
                if (saw != null)
                {
                    saw.StopMovement();
                }
                if (fireball != null)
                {
                    fireball.StopAnimation();
                }
            }
            isPaused = !isPaused;
        }

        private void Continue(object sender, RoutedEventArgs e)
        {
            pauseMenu.Continue();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            pauseMenu.OpenSettings();
        }

        private void FullScreen(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            pauseMenu.FullScreen(toggleButton.IsChecked == true);
        }

        private void GrassTextureClick(object sender, RoutedEventArgs e)
        {
            pauseMenu.GrassTextureClick();
        }

        private void GroundTextureClick(object sender, RoutedEventArgs e)
        {
            pauseMenu.GroundTextureClick();
        }

        private void MossyGroundTextureClick(object sender, RoutedEventArgs e)
        {
            pauseMenu.MossyGroundTextureClick();
        }

        private void LeaveSettings(object sender, RoutedEventArgs e)
        {
            pauseMenu.LeaveSettings();
        }

        private void Leave(object sender, RoutedEventArgs e)
        {
            pauseMenu.Leave();
        }

        private void ShowCursor()
        {
            Cursor = Cursors.Arrow; 
        }

        private void HideCursor()
        {
            Cursor = Cursors.None;
        }
    }
}