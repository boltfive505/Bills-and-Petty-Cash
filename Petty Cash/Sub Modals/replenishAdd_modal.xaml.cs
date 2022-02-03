using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;
using Petty_Cash.Classes;
using Petty_Cash.Objects;
using Petty_Cash.Objects.CheckWriter;
using Xceed.Wpf.Toolkit;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for replenishAdd_modal.xaml
    /// </summary>
    public partial class replenishAdd_modal : UserControl, IModalClosing
    {
        private bool requiresPassword;

        public replenishAdd_modal()
        {
            InitializeComponent();
        }

        public replenishAdd_modal(bool requiresPassword)
        {
            InitializeComponent();
            //this.requiresPassword = requiresPassword;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxItems();
        }

        private void LoadComboBoxItems()
        {
            var payeeTask = PettyCashContextHelper.GetPayeeListAsync(false);
            var checkTask = CheckWriterHelper.GetCheckListAync(false);
            Task.WaitAll(payeeTask, checkTask);

            PayeeValue.ItemsSource = new ObservableCollection<PayeeViewModel>(payeeTask.Result);
            BankCheckValue.ItemsSource = new ObservableCollection<CheckWriterCheckViewModel>(checkTask.Result.Where(i => i.Voucher == null).OrderByDescending(i => i.UpdatedDate));
        }

        public void ModalClosing(ModalClosingArgs e)
        {
            if (requiresPassword)
            {
                if (e.Result == ModalResult.Yes)
                {
                    if (ModalForm.ShowModal(new adminPassword_modal(), "Password Required", ModalButtons.YesNo) != ModalResult.Yes)
                        e.Cancel = true;
                }
            }
        }

        private bool BankCheckValue_SearchText(object item, string searchText)
        {
            string keyword = searchText.Trim();
            var check = item as CheckWriterCheckViewModel;
            return check.Id.ToString().StartsWith(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                check.CheckNumber.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                check.PayeeName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
