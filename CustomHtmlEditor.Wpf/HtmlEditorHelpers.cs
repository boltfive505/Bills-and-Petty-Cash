using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace CustomHtmlEditor.Wpf
{
    public static class HtmlHelpers
    {
        private const string WYSIWYG_EDITOR_RESOURCE_NAME = "CustomHtmlEditor.Wpf.html editor.zip";
        private const string SIMPLE_PREVIEW_RESOURCE_NAME = "CustomHtmlEditor.Wpf.simple preview.zip";
        private const string WYSIWYG_EDITOR_OUTPUT_NAME = "html editor";
        private const string SIMPLE_PREVIEW_OUTPUT_NAME = "simple preview";

        public static string ExtractWysiwygEditorFiles()
        {
            string outputDir = Path.Combine(Path.GetTempPath(), WYSIWYG_EDITOR_OUTPUT_NAME);
            ExtractZipContentFromResource(WYSIWYG_EDITOR_RESOURCE_NAME, outputDir);
            return Path.Combine(outputDir, "index.html");
        }

        public static string ExtractSimplePreviewFiles()
        {
            string outputDir = Path.Combine(Path.GetTempPath(), SIMPLE_PREVIEW_OUTPUT_NAME);
            ExtractZipContentFromResource(SIMPLE_PREVIEW_RESOURCE_NAME, outputDir);
            return Path.Combine(outputDir, "index.html");
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
