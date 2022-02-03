using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Petty_Cash.Classes
{
    public static class FileHelper
    {
        public enum FileNameRenameMode
        {
            NoRename, AddPrefix, AddSuffix, FullRename
        }

        public static string BasePath
        {
            get { return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); }
        }

        public static string CreateDirectory(string subdirectory)
        {
            EnsureValidSubdirectoryName(ref subdirectory);
            string path = Path.Combine(BasePath, subdirectory);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static void EnsureValidFileName(ref string fileName)
        {
            fileName = string.Join(" ", fileName.Split(Path.GetInvalidFileNameChars()));
        }

        private static void EnsureValidSubdirectoryName(ref string subdirectory)
        {
            subdirectory = string.Join(" ", subdirectory.Split(Path.GetInvalidPathChars()));
        }
        private static void EnsureUniqueFile(ref string file)
        {
            if (File.Exists(file))
            {
                int nextNum = 1;
                string directory = Path.GetDirectoryName(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                //check for incremental numbers
                string[] files = Directory.GetFiles(directory, fileName + "*" + extension, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    var nums = files.Select(f =>
                    {
                        Match m = Regex.Match(f, string.Format(@"{0} \((?<num>\d+)\){1}", fileName, extension));
                        if (m.Success)
                            return Convert.ToInt32(m.Groups["num"].Value);
                        else
                            return -1;
                    });
                    nextNum = nums.Max() + 1;
                }
                file = Path.Combine(directory, string.Format("{0} ({1}){2}", fileName, nextNum, extension));
            }
        }

        private static string RenameFileName(string fileName, string rename, FileNameRenameMode mode)
        {
            EnsureValidFileName(ref rename);
            switch (mode)
            {
                case FileNameRenameMode.AddPrefix: return rename + "_" + fileName;
                case FileNameRenameMode.AddSuffix: return fileName + "_" + rename;
                case FileNameRenameMode.FullRename: return rename;
                default: return fileName;
            }
        }

        public static string Upload(string srcFile, string subdirectory = "")
        {
            return Upload(srcFile, subdirectory, "", FileNameRenameMode.NoRename);
        }

        public static string Upload(string srcFile, string subdirectory, string rename, FileNameRenameMode renameMode)
        {
            string newDirectory = CreateDirectory(subdirectory);
            string destFile = Path.Combine(newDirectory, RenameFileName(Path.GetFileNameWithoutExtension(srcFile), rename, renameMode) + Path.GetExtension(srcFile));
            return UploadRaw(srcFile, destFile);
        }

        public static string UploadRaw(string srcFile, string destFile)
        {
            EnsureUniqueFile(ref destFile);
            File.Copy(srcFile, destFile, true);
            File.SetLastWriteTime(destFile, DateTime.Now);
            return destFile;
        }

        public static void Download(string destFile, string fileName, string subdirectory = "")
        {
            string directory = CreateDirectory(subdirectory);
            string srcFile = Path.Combine(directory, fileName);
            DownloadRaw(srcFile, destFile);
        }

        public static void DownloadRaw(string srcFile, string destFile)
        {
            File.Copy(srcFile, destFile, true);
            File.SetLastWriteTime(destFile, DateTime.Now);
        }

        public static void Delete(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static bool FileExist(string fileName, string subdirectory = "")
        {
            string file = GetFile(fileName, subdirectory);
            return File.Exists(file);
        }

        public static string GetFile(string fileName, string subdirectory = "")
        {
            if (fileName == null) return "";
            string directory = CreateDirectory(subdirectory);
            string file = Path.Combine(directory, fileName);
            return file;
        }

        public static void Select(string file)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/e, /select, \"{0}\"", file));
        }

        public static void Open(string file)
        {
            System.Diagnostics.Process.Start(file);
        }
    }
}
