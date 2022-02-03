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
    public partial class pettyCashAdminAddEdit_modal : UserControl, IModalClosing, IModalClosed, IModalCommand
    {
        private ICollectionView categoryItemList;
        private ICollectionView companyItemList;

        private List<CategoryViewModel> categoryList = new List<CategoryViewModel>();
        private List<CompanyViewModel> companyList = new List<CompanyViewModel>();

        private bool requiresPassword;

        public Action<ModalResult, object> ExecuteResult { get; set; }

        public pettyCashAdminAddEdit_modal()
        {
            InitializeComponent();
            Init();
        }

        public pettyCashAdminAddEdit_modal(bool requiresPassword)
        {
            InitializeComponent();
            //this.requiresPassword = requiresPassword;
            Init();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxItems();
        }

        private void Init()
        {
            categoryItemList = new CollectionViewSource() { Source = categoryList }.View;
            CategoryValue.ItemsSource = categoryItemList;

            companyItemList = new CollectionViewSource() { Source = companyList }.View;
            TinNumberValue.ItemsSource = companyItemList;
        }

        private void LoadComboBoxItems()
        {
            var categoryTask = PettyCashContextHelper.GetCategoryListAsync();
            var companyTask = PettyCashContextHelper.GetCompanyListAsync();
            Task.WaitAll(categoryTask, companyTask);

            categoryList.AddRange(categoryTask.Result);
            companyList.AddRange(companyTask.Result);

            categoryItemList.Refresh();
            companyItemList.Refresh();
        }

        public void ModalClosing(ModalClosingArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                //check for errors
                if (TinNumberValue.GetBindingExpression(ComboBox.SelectedItemProperty).HasError)
                {
                    System.Windows.MessageBox.Show("VAT is applied. Must require TIN #", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                    return;
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
        }

        public void ModalClosed(ModalClosedArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                //check if new input for payee, category, and company info
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

        private void TinNumberValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtbox = e.OriginalSource as TextBox;
            ComboBox cbox = sender as ComboBox;
            bool validate = string.IsNullOrEmpty(txtbox.Text);
            (this.DataContext as PettyCashViewModel).SetTinRequired(validate);
        }
    }
}
