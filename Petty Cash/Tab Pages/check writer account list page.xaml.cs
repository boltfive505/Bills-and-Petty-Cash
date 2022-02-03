using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Data.Entity;
using Petty_Cash.Objects.CheckWriter;
using Petty_Cash.Model;
using Petty_Cash.Sub_Modals.CheckWriter;
using Petty_Cash.Classes;
using bolt5.ModalWpf;
using bolt5.CloneCopy;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for category_list_page.xaml
    /// </summary>
    public partial class check_writer_account_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
            public bool ShowInActive { get; set; }
        }

        private ICollectionView accountView;
        private List<CheckWriterAccountViewModel> accountList = new List<CheckWriterAccountViewModel>();
        private FilterGroup filters;

        public check_writer_account_list_page()
        {
            InitializeComponent();
            accountView = new CollectionViewSource() { Source = accountList }.View;
            accountView.SortDescriptions.Add(new SortDescription("AccountName", ListSortDirection.Ascending));
            accountView.Filter = x => DoFilterAccount(x as CheckWriterAccountViewModel);
            accountDatagrid.ItemsSource = accountView;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            accountList.Clear();
            accountList.AddRange(CheckWriterHelper.GetAccountsListAync().GetResult());
            accountView.Refresh();
        }
        
        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            accountView.Refresh();
        }

        private bool DoFilterAccount(CheckWriterAccountViewModel i)
        {
            bool flag = true;
            //keyword
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
            {
                string keyword = filters.FilterKeyword.Trim();
                flag &= i.AccountName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        i.AccountNumber.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            //show inactive
            if (!filters.ShowInActive)
                flag &= i.IsActive;
            return flag;
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            bool isEdit = true;
            string title = "Edit Account";
            var account = (sender as FrameworkElement).DataContext as CheckWriterAccountViewModel;
            if (account == null)
            {
                isEdit = false;
                title = "Add Account";
                account = new CheckWriterAccountViewModel();
            }
            var modal = new account_add_modal();
            var clone = account.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, title, ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(account);
                _ = CheckWriterHelper.AddAccountAsync(account);
                if (!isEdit)
                    accountList.Add(account);
                accountView.Refresh();
            }
        }
    }
}
