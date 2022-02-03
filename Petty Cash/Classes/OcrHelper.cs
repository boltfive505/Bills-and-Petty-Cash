using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using Tesseract;

namespace Petty_Cash.Classes
{
    //Any image readable by Leptonica is supported in Tesseract including BMP, PNM, PNG, JFIF, JPEG, and TIFF. GIF is not supported.
    public static class OcrHelper
    {
        private static string[] supportedImageFormats = { ".png", ".jpg", ".jpeg" };

        private static TesseractEngine CreateEngine()
        {
            return new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        }

        public static string GetTextFromFile(string file)
        {
            string extension = Path.GetExtension(file).ToLower();
            if (extension == ".pdf")
            {
                //file is pdf
                BitmapImage pdfImage = PdfHelper.GetImageFromPdf(file);
                return GetTextFromImage(pdfImage);
            }
            else if (supportedImageFormats.Contains(extension))
            {
                //file is image
                using (FileStream fs = File.OpenRead(file))
                {
                    MemoryStream ms = new MemoryStream();
                    fs.CopyTo(ms);
                    return GetTextFromData(ms.ToArray());
                }
            }
            else
            {
                //file not supported
                return "ERROR: File not supported";
            }
        }

        public static string GetTextFromImage(BitmapImage img)
        {
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return GetTextFromData(ms.ToArray());
            }
        }

        public static string GetTextFromData(byte[] imageData)
        {
            using (var engine = OcrHelper.CreateEngine())
            {
                using (var img = Pix.LoadFromMemory(imageData))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText();
                    }
                }
            }
        }

        public static void OpenTextToNotepad(string text)
        {
            string file = Path.Combine(Path.GetTempPath(), string.Format("extracted text_{0:yyyy-MM-dd-hhmmss}.txt", DateTime.Now));
            File.WriteAllText(file, text);
            FileHelper.Open(file);
        }
    }
}
