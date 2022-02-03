using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Petty_Cash
{
    public static class Logs
    {
        public static void WriteDebug(string msg)
        {
            WriteLog(msg);
        }

        public static void WriteException(Exception ex)
        {
            WriteLog(ex.ToString());
        }

        private static void WriteLog(string msg)
        {
            string file = Path.Combine(GetLogFolder(), "logs.txt");
            string content = string.Format("[{0:MM/dd/yyyy hhh:mm:ss}] {1}", DateTime.Now, msg);
            try
            {
                File.AppendAllText(file, content + Environment.NewLine);
            }
            catch (IOException)
            { }
        }

        private static string GetLogFolder()
        {
            string dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "log");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
