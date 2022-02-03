using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Text.RegularExpressions;
using Petty_Cash.Objects;
using Petty_Cash.Model;
using Petty_Cash.Classes;
using bolt5.CustomControls;
using bolt5.ModalWpf;
using Xceed.Wpf.Toolkit;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for pettyCashAddEdit_modal.xaml
    /// </summary>
    public partial class pettyCashAddEdit_modal : UserControl, IModalClosing, IModalClosed, IModalCommand
    {
        private ICollectionView categoryItemList;
        private ICollectionView companyItemList;
        private ICollectionView voucherItemHistoryList;

        private List<CategoryViewModel> categoryList = new List<CategoryViewModel>();
        private List<CompanyViewModel> companyList = new List<CompanyViewModel>();
        private List<PettyCashViewModel> voucherHistoryList = new List<PettyCashViewModel>();

        private bool requiresPassword;

        public Action<ModalResult, object> ExecuteResult { get; set; }

        public pettyCashAddEdit_modal()
        {
            InitializeComponent();
            Init();
        }

        public pettyCashAddEdit_modal(bool requiresPassword)
        {
            InitializeComponent();
            //this.requiresPassword = requiresPassword;
            Init();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxItems();
            SetVoucherHistory();
            //refresh voucher info
            VoucherViewModel voucherVm = (this.DataContext as PettyCashViewModel).Voucher;
            if (voucherVm != null)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await PettyCashContextHelper.ReloadVoucherInfoAsync(voucherVm);
                    await Dispatcher.BeginInvoke(new Action(() => BalanceValue.GetBindingExpression(DecimalUpDown.ValueProperty).UpdateTarget()));
                });
            }
        }

        private void Init()
        {
            categoryItemList = new CollectionViewSource() { Source = categoryList }.View;
            CategoryValue.ItemsSource = categoryItemList;

            companyItemList = new CollectionViewSource() { Source = companyList }.View;
            TinNumberValue.ItemsSource = companyItemList;

            voucherItemHistoryList = new CollectionViewSource() { Source = voucherHistoryList }.View;
            historyDg.ItemsSource = voucherItemHistoryList;
        }

        private void LoadComboBoxItems()
        {
            var payeeTask = PettyCashContextHelper.GetPayeeListAsync(false);
            var categoryTask = PettyCashContextHelper.GetCategoryListAsync();
            var companyTask = PettyCashContextHelper.GetCompanyListAsync();
            var voucherTask = PettyCashContextHelper.GetOpenVoucherListAsync();
            Task.WaitAll(payeeTask, categoryTask, companyTask, voucherTask);

            categoryList.AddRange(categoryTask.Result);
            companyList.AddRange(companyTask.Result);

            categoryItemList.Refresh();
            companyItemList.Refresh();
        }

        private void SetVoucherHistory()
        {
            voucherHistoryList.Clear();
            VoucherViewModel voucherVm = (this.DataContext as PettyCashViewModel).Voucher as VoucherViewModel;
            if (voucherVm != null)
            {
                var task = PettyCashContextHelper.GetPettyCashListFromVoucherIdAsync(voucherVm.Id);
                task.Wait();
                voucherHistoryList.AddRange(task.Result);
            }
            voucherItemHistoryList.Refresh();
        }

        public void ModalClosing(ModalClosingArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                //check for errors
                if (BalanceValue.GetBindingExpression(DecimalUpDown.ValueProperty).HasError)
                {
                    System.Windows.MessageBox.Show("Amount is going over the Remaining Amount", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                    return;
                }
                else if (TinNumberValue.GetBindingExpression(ComboBox.SelectedItemProperty).HasError)
                {
                    System.Windows.MessageBox.Show("VAT is applied. Must require TIN #", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                    return;
                }
               
                if (e.Key != null && e.Key.Equals("CloseVoucher"))
                {
                    string msg = "Do you want to CLOSE this Voucher?";
                    if ((BalanceValue.Value ?? 0) > 0)
                    {
                        //voucher has balance amount, also warn the user to receive the balance
                        msg += "\n\nNOTE: This voucher still has Balance. Make sure to receive the Balance Amount of " + (BalanceValue.Value ?? 0).ToString("N2") + " from \"" + PayeeValue.Text + "\" to close this voucher.";
                    }
                    if (System.Windows.MessageBox.Show(msg, "Save & Close Voucher", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (requiresPassword)
                {
                    if (ModalForm.ShowModal(new adminPassword_modal(), "Password Required", ModalButtons.YesNo) != ModalResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            else if (e.Result == ModalResult.Cancel && e.Key.Equals("ClearVoucher"))
            {
                if (System.Windows.MessageBox.Show("Do you want to CANCEL this Voucher?\n(This will delete all previous records)", "Cancel Voucher", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        public void ModalClosed(ModalClosedArgs e)
        {
            if (e.Result == ModalResult.Save) //result == ModalResult.New
            {
                //check if new input
                CheckCategoryIsNew();
                CheckCompanyIfNew();
            }
        }

        private void OpenCamera_btn_Click(object sender, RoutedEventArgs e)
        {
            openCamera_modal camera = new openCamera_modal();
            if (ModalForm.ShowModal(camera, "Capture Payment File", ModalButtons.YesNo) == ModalResult.Yes)
            {
                PaymentFileValue.SetValue(FileAttachment.FileNameProperty, camera.GetCapturedImageFile());
            }
        }

        private void CheckCategoryIsNew()
        {
            if (CategoryValue.SelectedItem == null && !string.IsNullOrWhiteSpace(CategoryValue.Text))
            {
                var categoryVm = new CategoryViewModel() { CategoryName = CategoryValue.Text };
                var task = PettyCashContextHelper.AddUpdateCategoryAsync(categoryVm);
                task.Wait();
                categoryList.Add(categoryVm);
                categoryItemList.Refresh();
                CategoryValue.SetValue(ComboBox.SelectedItemProperty, categoryVm);
            }
        }

        private void CheckCompanyIfNew()
        {
            if (TinNumberValue.SelectedItem == null && !string.IsNullOrWhiteSpace(TinNumberValue.Text))
            {
                var companyVm = new CompanyViewModel() { CompanyName = CompanyValue.Text, TinNumber = TinNumberValue.Text, Address = AddressValue.Text };
                var task = PettyCashContextHelper.AddUpdateCompanyAsync(companyVm);
                task.Wait();
                companyList.Add(companyVm);
                companyItemList.Refresh();
                TinNumberValue.SetValue(ComboBox.SelectedItemProperty, companyVm);
            }
        }

        private void TinNumberValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CompanyViewModel company = TinNumberValue.SelectedItem as CompanyViewModel;
            if (company == null)
            {
                CompanyValue.Text = string.Empty;
                AddressValue.Text = string.Empty;
            }
            else
            {
                CompanyValue.Text = company.CompanyName;
                AddressValue.Text = company.Address;
            }
        }

        //private void VoucherValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    voucherHistoryList.Clear();
        //    VoucherViewModel voucherVm = (this.DataContext as PettyCashViewModel).Voucher as VoucherViewModel;
        //    if (voucherVm != null)
        //    {
        //        var task = PettyCashContextHelper.GetPettyCashListFromVoucherIdAsync(voucherVm.Id);
        //        task.Wait();
        //        voucherHistoryList.AddRange(task.Result);
        //    }
        //    voucherItemHistoryList.Refresh();
        //}

        private void SaveAndKeep_btn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteResult(ModalResult.Save, "AddMore");
        }

        private void Cancel_btn_Click(object sender, RoutedEventArgs e)
        {
            ExecuteResult(ModalResult.Cancel, "ClearVoucher");
        }

        private void TinNumberValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtbox = e.OriginalSource as TextBox;
            ComboBox cbox = sender as ComboBox;
            bool validate = string.IsNullOrEmpty(txtbox.Text);
            (this.DataContext as PettyCashViewModel).SetTinRequired(validate);
        }
    }
}
