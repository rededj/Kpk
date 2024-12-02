using System.IO;
using System.Windows.Media.Imaging;

namespace VampireSurvivors
{
    public static class ImageHelper
    {
        public static BitmapImage LoadImageFromBytes(byte[] imageBytes)
        {
            using (var stream = new MemoryStream(imageBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        public static BitmapImage GetCurrentWalkImage(int currentPlayerFrame, BitmapImage walkImage1, BitmapImage walkImage2, BitmapImage walkImage3, BitmapImage walkImage4, BitmapImage idleImage)
        {
            if (currentPlayerFrame == 0)
                return walkImage1;
            else if (currentPlayerFrame == 1)
                return walkImage2;
            else if (currentPlayerFrame == 2)
                return walkImage3;
            else if (currentPlayerFrame == 3)
                return walkImage4;
            else
                return idleImage;
        }
    }
}