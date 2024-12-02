using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace VampireSurvivors
{
    public partial class MainWindow : Window
    {
        private bool isFullScreen = false;
        byte[] backgroundImagePath = Properties.Resources.GrassTexture;
        PauseMenu pauseMenu;
        public MainWindow()
        {
            InitializeComponent();
            GoFullScreen();
        }

        private void GoFullScreen()
        {
            WindowState = WindowState.Maximized;
        }

        public void Play(object sender, RoutedEventArgs e)
        {
            Game gameShow = new Game(isFullScreen, backgroundImagePath);
            gameShow.Show();
            Thread.Sleep(250);
            this.Close();
        }

        public void Settings(object sender, RoutedEventArgs e)
        {
            SettingsBox.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Collapsed;
        }

        public void FullScreen(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;

            if (toggleButton != null)
            {
                if (toggleButton.IsChecked == true)
                {
                    WindowStyle = WindowStyle.None; 
                    WindowState = WindowState.Normal;
                    WindowState = WindowState.Maximized;
                    isFullScreen = true;
                }
                else
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                    WindowState = WindowState.Maximized;
                    Topmost = false;
                    isFullScreen = false;   
                }
            }
        }

        public void GrassTextureClick(object sender, RoutedEventArgs e)
        {
            ChangeBackground(Properties.Resources.GrassTexture);
            backgroundImagePath= Properties.Resources.GrassTexture;
        }

        public void GroundTextureClick(object sender, RoutedEventArgs e)
        {
            ChangeBackground(Properties.Resources.GroundTexture);
            backgroundImagePath = Properties.Resources.GroundTexture;
        }

        public void MossyGroundTextureClick(object sender, RoutedEventArgs e)
        {
            ChangeBackground(Properties.Resources.MossyGroundTexture);
            backgroundImagePath = Properties.Resources.MossyGroundTexture;
        }

        public void ChangeBackground(byte[] imageBytes)
        {
            BitmapImage image = ImageHelper.LoadImageFromBytes(imageBytes);
            MainGrid.Background = new ImageBrush
            {
                ImageSource = image,
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 300, 300),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        public void Back(object sender, RoutedEventArgs e)
        {
            Menu.Visibility = Visibility.Visible;
            SettingsBox.Visibility = Visibility.Collapsed;
        }

        public void End(object sender, RoutedEventArgs e)
        {
            LeaveBox.Visibility = Visibility.Visible;
            Menu.IsEnabled = false;
        }

        private void YesClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            LeaveBox.Visibility = Visibility.Collapsed;
            Menu.IsEnabled = true;
        }
    }
}
