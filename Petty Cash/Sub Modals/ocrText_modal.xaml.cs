using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for ocrText_modal.xaml
    /// </summary>
    public partial class ocrText_modal : UserControl
    {
        public ocrText_modal(string text)
        {
            InitializeComponent();
            ocrTxt.Text = text;
        }

        private void CopyText_btn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ocrTxt.Text);
        }
    }
}
