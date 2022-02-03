using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace Petty_Cash.Classes
{
    //
    //https://ourcodeworld.com/articles/read/629/how-to-create-and-extract-zip-files-compress-and-decompress-zip-with-sharpziplib-with-csharp-in-winforms
    //
    public static class BackupRestoreHelper
    {
        private static string[] _databaseFiles =
        {
            @".\data\bills.db",
            @".\data\petty cash.db",
            @".\data\tutorials.db",
            @".\data\check writer.db",
        };

        private static string GetBackupPath()
        {
            string dir = @".\data\backups";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }

        public static void DoBackup()
        {
            ZipOutputStream output = null;
            try
            {
                string outputFile = Path.Combine(BackupRestoreHelper.GetBackupPath(), string.Format("backup_bills and cash_{0}.bk", DateTime.Now.ToString("yyyy-MM-dd_hhh-mm-ss")));
                using (output = new ZipOutputStream(File.Create(outputFile)))
                {
                    output.SetLevel(9);
                    byte[] buffer = new byte[4096];
                    foreach (string f in BackupRestoreHelper._databaseFiles)
                    {
                        if (!File.Exists(f)) continue;
                        ZipEntry entry = new ZipEntry(Path.GetFileName(f));
                        entry.DateTime = DateTime.Now;
                        output.PutNextEntry(entry);
                        using (FileStream fs  = File.OpenRead(f))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                output.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }
                }
                System.Windows.MessageBox.Show("Dome making backup", "Backup", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logs.WriteException(ex);
                System.Windows.MessageBox.Show("An error occured while making backup", "Backup", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                if (output != null)
                {
                    output.Finish();
                    output.Close();
                }
            }
        }

        public static IEnumerable<string> GetBackupFiles()
        {
            return Directory.GetFiles(BackupRestoreHelper.GetBackupPath(), "*.bk", SearchOption.TopDirectoryOnly);
        }

        public static void DoRestore(string backupFile)
        {
            ZipFile file = null;
            try
            {
                FileStream fs = File.OpenRead(backupFile);
                file = new ZipFile(fs);

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
                    String fullZipToPath = Path.Combine(@".\data", entryFileName);
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
                System.Windows.MessageBox.Show("Dome restore data", "Restore", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logs.WriteException(ex);
                System.Windows.MessageBox.Show("An error occured while restoring data", "Restore", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
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
