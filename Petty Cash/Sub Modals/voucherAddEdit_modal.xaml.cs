using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;
using Xceed.Wpf.Toolkit;
using Petty_Cash.Classes;
using Petty_Cash.Objects;
using System.ComponentModel;
using System.Windows.Data;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for voucherAddEdit_modal.xaml
    /// </summary>
    public partial class voucherAddEdit_modal : UserControl, IModalClosed
    {
        private ICollectionView payeeItemList;
        private List<PayeeViewModel> payeeList = new List<PayeeViewModel>();
        private bool requiresPassword;

        public voucherAddEdit_modal()
        {
            InitializeComponent();
            payeeItemList = new CollectionViewSource() { Source = payeeList }.View;
            PayeeValue.ItemsSource = payeeItemList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxItems();
        }

        private void LoadComboBoxItems()
        {
            var payeeTask = PettyCashContextHelper.GetPayeeListAsync(false);
            Task.WaitAll(payeeTask);

            payeeList.AddRange(payeeTask.Result.Where(i => i.Id != -2)); //exclude admin expense payee
            payeeItemList.Refresh();
        }

        public void ModalClosed(ModalClosedArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                CheckPayeeIfNew();
            }
        }

        private void CheckPayeeIfNew()
        {
            if (PayeeValue.SelectedItem == null && !string.IsNullOrWhiteSpace(PayeeValue.Text))
            {
                var payeeVm = new PayeeViewModel() { PayeeName = PayeeValue.Text };
                var task = PettyCashContextHelper.AddUpdatePayeeAsync(payeeVm);
                task.Wait();
                payeeList.Add(payeeVm);
                payeeItemList.Refresh();
                PayeeValue.SetValue(ComboBox.SelectedItemProperty, payeeVm);
            }
        }
    }
}
