using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace VampireSurvivors
{
    public class PauseMenu
    {
        private Game game;

        public PauseMenu(Game game)
        {
            this.game = game;
        }

        public void Continue()
        {
            game.Pause();
            game.PauseMenu.Visibility = Visibility.Collapsed;
            game.Menu.Visibility = Visibility.Collapsed;
        }

        public void OpenSettings()
        {
            game.PauseMenu.Visibility = Visibility.Collapsed;
            game.Settings.Visibility = Visibility.Visible;
        }

        public void FullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                game.GoFullScreen();
            }
            else
            {
                game.GoMaximisedScreen();
            }
        }

        public void GrassTextureClick()
        {
            ChangeBackground(Properties.Resources.GrassTexture);
        }

        public void GroundTextureClick()
        {
            ChangeBackground(Properties.Resources.GroundTexture);
        }

        public void MossyGroundTextureClick()
        {
            ChangeBackground(Properties.Resources.MossyGroundTexture);
        }

        public void ChangeBackground(byte[] imageBytes)
        {
            BitmapImage image = ImageHelper.LoadImageFromBytes(imageBytes);
            game.GameCanvas.Background = new ImageBrush
            {
                ImageSource = image,
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 300, 300),
                ViewportUnits = BrushMappingMode.Absolute
            };
        }

        public void LeaveSettings()
        {
            game.Settings.Visibility = Visibility.Collapsed;
            game.PauseMenu.Visibility = Visibility.Visible;
        }

        public void Leave()
        {
            Application.Current.Shutdown();
        }
    }
}
