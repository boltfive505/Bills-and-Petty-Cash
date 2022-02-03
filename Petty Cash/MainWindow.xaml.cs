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

namespace Petty_Cash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InstanceManager.BroadcastReceived += InstanceManager_BroadcastReceived;
        }

        private void InstanceManager_BroadcastReceived()
        {
            this.WindowState = WindowState.Maximized;
            bool topmost = this.Topmost;
            this.Topmost = true;
            this.Topmost = topmost;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            InstanceManager.AddHookWndProc(this);
        }

        private void TabControl_LiquidateVoucher(object sender, Tab_Pages.LiquidateVoucherEventArgs e)
        {
            tabControl1.SelectedItem = pettyCashTab;
            pettyCashPage.AddPettyCash(e.Voucher);
        }

        private void tabControl1_SelectBill(object sender, Tab_Pages.SelectBillEventArgs e)
        {
            tabControl1.SelectedItem = billsPaymentTab;
            billsPaymentPage.SelectBill(e.Bill);
        }

        
    }
}
