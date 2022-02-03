using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Petty_Cash.Classes
{
    public static class MiscHelper
    {
        private const string ADMIN_PASSWORD_DEFAULT = "123456";

        public static string GetAdminPassword()
        {
            string file = FileHelper.GetFile("pw.txt", "bin");
            if (!File.Exists(file))
                File.WriteAllText(file, ADMIN_PASSWORD_DEFAULT);
            return File.ReadAllText(file);
        }
    }
}
