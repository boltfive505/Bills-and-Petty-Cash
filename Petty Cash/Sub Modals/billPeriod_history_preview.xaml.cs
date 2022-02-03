using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;
using Xceed.Wpf.Toolkit;
using Petty_Cash.Objects.Bills;
using Petty_Cash.Classes;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for billPeriodAddEdit_modal.xaml
    /// </summary>
    public partial class billPeriod_history_preview : UserControl
    {
        private ICollectionView paymentHistoryView;
        private List<BillPaymentViewModel> paymentList = new List<BillPaymentViewModel>();

        public billPeriod_history_preview()
        {
            InitializeComponent();
            paymentHistoryView = new CollectionViewSource() { Source = paymentList }.View;
            paymentHistoryView.SortDescriptions.Add(new SortDescription("PeriodDate", ListSortDirection.Descending));
            payemntHistoryDataGrid.ItemsSource = paymentHistoryView;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            paymentList.Clear();
            paymentHistoryView.Refresh();
            BillPeriodViewModel period = this.DataContext as BillPeriodViewModel;
            if (period != null)
            {
                var task = BillsPeriodContextHelper.GetBillPaymentListAsync(period.Id);
                task.ContinueWith(t =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        paymentList.AddRange(t.Result);
                        paymentHistoryView.Refresh();
                    }), System.Windows.Threading.DispatcherPriority.Background);
                });
            }
        }

        private void OpenFile_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (string)(sender as FrameworkElement).Tag;
            string file = FileHelper.GetFile(fileName, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                System.Windows.MessageBox.Show("ERROR: File not found", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
