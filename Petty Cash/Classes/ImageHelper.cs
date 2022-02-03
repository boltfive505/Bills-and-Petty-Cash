using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace Petty_Cash.Classes
{
    public static class ImageHelper
    {
        public static Size Resize(Size actualSize, Size maxSize)
        {
            double sourceWidth = actualSize.Width;
            double sourceHeight = actualSize.Height;
            double maxWidth = maxSize.Width;
            double maxHeight = maxSize.Height;

            double nPercent;
            double nPercentW;
            double nPercentH;

            nPercentW = maxWidth / sourceWidth;
            nPercentH = maxHeight / sourceHeight;
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
            }
            else
            {
                nPercent = nPercentW;
            }
            double destWidth = sourceWidth * nPercent;
            double destHeight = sourceHeight * nPercent;

            return new Size(destWidth, destHeight);
        }

        public static BitmapImage ImageToBitmapImage(System.Drawing.Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
        }
    }
}
