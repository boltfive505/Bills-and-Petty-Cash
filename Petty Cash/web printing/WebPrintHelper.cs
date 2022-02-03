using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Petty_Cash.web_printing.objects;
using bolt5.ModalWpf;

namespace Petty_Cash.web_printing
{
    public static class WebPrintHelper
    {
        public static string GenerateXmlForBillList(IEnumerable<Objects.Bills.BillPeriodViewModel> list)
        {
            List<BillXml> billList = list.Select(i => new BillXml()
            {
                BillerName = i.BillerName,
                Description = i.Description,
                CategoryName = i.Category?.CategoryName
            }).ToList();
            return SerializeXml(billList);
        }

        private static string SerializeXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, obj);
                string xml = Encoding.UTF8.GetString(ms.ToArray());
                return xml;
            }
        }

        public static void Print(string path, string title)
        {
            var print = new web_printing_modal();
            print.WebPagePath = path;
            ModalForm.ShowModal(print, title);
        }

        public static string CreatePathForPrintableXmlWithStylesheetAndWebPagePack(string xml, string styleSheetName, string webpagePackName)
        {
            string zipPackResourcePath = "Petty_Cash.web_printing.webpage_packs." + webpagePackName + ".zip";
            string packOutputPath = Path.Combine(Path.GetTempPath(), webpagePackName);
            WebPrintHelper.ExtractZipContentFromResource(zipPackResourcePath, packOutputPath);
            string printOutputPath = Path.Combine(packOutputPath, "index.html");
            DoCreatePathForPrintableXmlWithStyleSheet(xml, styleSheetName, printOutputPath);
            return printOutputPath;
        }

        public static string CreatePathForPrintableXmlWithStyleSheet(string xml, string styleSheetName)
        {
            string tempFile = Path.GetTempFileName();
            DoCreatePathForPrintableXmlWithStyleSheet(xml, styleSheetName, tempFile);
            return tempFile;
        }

        private static void DoCreatePathForPrintableXmlWithStyleSheet(string xml, string styleSheetName, string targetPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XslCompiledTransform xsl = new XslCompiledTransform();
            XmlReaderSettings settings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse };
            Uri styleSheetUri = new Uri("pack://application:,,,/Bills and Cash;component/web printing/stylesheets/" + styleSheetName + ".xsl", UriKind.RelativeOrAbsolute);
            XmlReader reader = XmlReader.Create(Application.GetResourceStream(styleSheetUri).Stream, settings);
            xsl.Load(reader);
            XmlTextWriter writer = new XmlTextWriter(targetPath, Encoding.UTF8);
            xsl.Transform(doc, null, writer);
        }

        //
        //https://ourcodeworld.com/articles/read/629/how-to-create-and-extract-zip-files-compress-and-decompress-zip-with-sharpziplib-with-csharp-in-winforms
        //
        private static void ExtractZipContentFromResource(string zipResourcePath, string outputFolder)
        {
            ZipFile file = null;
            try
            {
                Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(zipResourcePath);
                //FileStream fs = File.OpenRead(FileZipPath);
                file = new ZipFile(resourceStream);

                foreach (ZipEntry zipEntry in file)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }

                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // 4K is optimum
                    byte[] buffer = new byte[4096];
                    Stream zipStream = file.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outputFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            catch (IOException)
            { }
            finally
            {
                if (file != null)
                {
                    file.IsStreamOwner = true; // Makes close also shut the underlying stream
                    file.Close(); // Ensure we release resources
                }
            }
        }
    }
}
