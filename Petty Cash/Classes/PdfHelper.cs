using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;

namespace Petty_Cash.Classes
{
    public static class PdfHelper
    {
        private static GhostscriptVersionInfo gsVersion;

        static PdfHelper()
        {
            SetGhostscriptDirectory();
        }

        private static void SetGhostscriptDirectory()
        {
            string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gs");
            System.Diagnostics.Debug.WriteLine(dir);
            gsVersion = new GhostscriptVersionInfo(new Version(0, 0, 0), dir + @"\gsdll32.dll", dir + @"\gsdll32.lib", GhostscriptLicense.GPL);
        }

        public static BitmapImage GetImageFromPdf(string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (var rasterizer = new GhostscriptRasterizer())
                {
                    rasterizer.Open(fs, gsVersion, false);
                    System.Drawing.Image img = rasterizer.GetPage(300, 1);
                    return ImageHelper.ImageToBitmapImage(img);
                }
            }
        }
    }
}
