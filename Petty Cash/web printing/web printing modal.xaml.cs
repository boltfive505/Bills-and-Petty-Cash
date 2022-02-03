using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Petty_Cash.web_printing
{
    /// <summary>
    /// Interaction logic for web_printing_modal.xaml
    /// </summary>
    public partial class web_printing_modal : UserControl
    {
        public string WebPagePath { get; set; }

        public web_printing_modal()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            printBrowser.Navigate(WebPagePath);
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            IServiceProvider serviceProvider = null;
            if (printBrowser.Document != null)
            {
                serviceProvider = (IServiceProvider)printBrowser.Document;
            }

            Guid serviceGuid = SID_SWebBrowserApp;
            Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;

            object NullValue = null;

            SHDocVw.IWebBrowser2 target = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
            target.ExecWB(SHDocVw.OLECMDID.OLECMDID_PRINTPREVIEW, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref NullValue, ref NullValue);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }
        static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
    }
}
