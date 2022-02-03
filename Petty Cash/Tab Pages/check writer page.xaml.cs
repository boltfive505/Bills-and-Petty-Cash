using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Petty_Cash.Classes;
using Petty_Cash.Objects.CheckWriter;
using Petty_Cash.Sub_Modals.CheckWriter;
using bolt5.ModalWpf;
using bolt5.CloneCopy;
using PropertyChanged;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for check_writer_page.xaml
    /// </summary>
    public partial class check_writer_page : Page
    {
        public class BankAccountPair : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public CheckWriterBankViewModel Bank { get; private set; }
            public CheckWriterAccountViewModel Account { get; private set; }

            public bool IsBank { get; private set; }
            public bool IsExpanded { get; set; } = true;
            public bool HasChildren { get; set; }
            public bool IsVisible { get; set; } = true;

            private BankAccountPair _parent;
            [AlsoNotifyFor("HierarchyPath")]
            public BankAccountPair Parent
            {
                get { return _parent; }
                set
                {
                    //remove event
                    if (_parent != null)
                        _parent.PropertyChanged -= _parent_PropertyChanged;
                    _parent = value;
                    //add event
                    if (_parent != null)
                        _parent.PropertyChanged += _parent_PropertyChanged;
                    OnParentChanged();
                }
            }

            public int Id { get { return IsBank ? Bank.Id : Account.Id; } }
            public object Data { get { return IsBank ? (object)Bank : (object)Account; } }
            public string Title { get { return IsBank ? Bank.BankName : Account.AccountNumber; } }
            public string HierarchyPath
            { 
                get
                {
                    if (IsBank)
                        return Bank.BankName;
                    else if (Parent != null)
                        return Parent.Bank.BankName + "/" + Account.AccountNumber;
                    else return Account.AccountNumber;
                }
            }

            public BankAccountPair(CheckWriterBankViewModel bank)
            {
                this.Bank = bank;
                this.IsBank = true;
            }

            public BankAccountPair(CheckWriterAccountViewModel account)
            {
                this.Account = account;
                this.IsBank = false;
            }

            private void _parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                OnParentChanged();
            }

            private void OnParentChanged()
            {
                IsVisible = !IsBank && Parent != null ? Parent.IsExpanded : true;
            }
        }

        public class SelectionGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public BankAccountPair SelectedBankAccountPair { get; set; }
        }

        public static readonly DependencyProperty SelectionsProperty = DependencyProperty.Register(nameof(Selections), typeof(SelectionGroup), typeof(check_writer_page));
        public SelectionGroup Selections
        {
            get { return (SelectionGroup)GetValue(SelectionsProperty); }
            set { SetValue(SelectionsProperty, value); }
        }

        private ICollectionView bankAccountView;
        private ICollectionView checkView;
        private List<BankAccountPair> bankAccountList = new List<BankAccountPair>();
        private List<CheckWriterCheckViewModel> checkList = new List<CheckWriterCheckViewModel>();

        public check_writer_page()
        {
            InitializeComponent();
            bankAccountView = new CollectionViewSource() { Source = bankAccountList }.View;
            bankAccountView.SortDescriptions.Add(new SortDescription("HierarchyPath", ListSortDirection.Ascending));
            banks_accounts_DataGrid.ItemsSource = bankAccountView;

            checkView = new CollectionViewSource() { Source = checkList }.View;
            checkView.SortDescriptions.Add(new SortDescription("UpdatedDate", ListSortDirection.Descending));
            checkView.Filter = x => DoFilterCheck(x as CheckWriterCheckViewModel);
            checkDataGrid.ItemsSource = checkView;

            Selections = new SelectionGroup();
            Selections.PropertyChanged += Selections_PropertyChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            bankAccountList.Clear();
            checkList.Clear();
            bankAccountView.Refresh();
            checkView.Refresh();
            var bankTask = CheckWriterHelper.GetBankListAsync(false);
            var accountTask = CheckWriterHelper.GetAccountsListAync(false);
            var checkTask = LoadDataTask(checkList, CheckWriterHelper.GetCheckListAync(false), checkView);
            Task.WaitAll(bankTask, accountTask);

            bankAccountList.AddRange(bankTask.Result.Select(i => new BankAccountPair(i)));
            bankAccountList.AddRange(accountTask.Result.Select(i => new BankAccountPair(i)));
            SetBankAccountHierarchy();
            bankAccountView.Refresh();
        }

        private async Task LoadDataTask<T>(List<T> list, Task<IEnumerable<T>> task, ICollectionView view)
        {
            list.Clear();
            var result = await task;
            list.AddRange(result);
            Dispatcher.Invoke(new Action(() => view?.Refresh()), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SetBankAccountHierarchy()
        {
            List<BankAccountPair> list = bankAccountList;
            //get all banks
            var bankList = list.Where(i => i.IsBank);
            //set account parent
            foreach (var b in bankList)
            {
                var accList = list.Where(i => !i.IsBank && i.Account.Bank.Id == b.Bank.Id);
                if (accList.Count() > 0)
                {
                    b.HasChildren = true;
                    foreach (var acc in accList)
                    {
                        acc.Parent = b;
                        acc.Account.Bank = b.Bank;
                    }
                }
            }
        }

        private void Selections_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            checkView.Refresh();
        }

        private bool DoFilterCheck(CheckWriterCheckViewModel i)
        {
            bool flag = true;
            if (Selections.SelectedBankAccountPair != null)
            {
                bool isBank = Selections.SelectedBankAccountPair.IsBank;
                int id = Selections.SelectedBankAccountPair.Id;
                if (isBank)
                    flag &= i.Account.Bank.Id == id; //selection is bank
                else
                    flag &= i.Account.Id == id; //selection is acccount
            }
            return flag;
        }

        private void AddBank_Click(object sender, RoutedEventArgs e)
        {
            var isEdit = true;
            var title = "Edit Bank";
            var pair = (sender as FrameworkElement).DataContext as BankAccountPair;
            var bank = pair?.Bank;
            if (bank == null)
            {
                isEdit = false;
                title = "Add Bank";
                bank = new CheckWriterBankViewModel();
            }
            var modal = new bank_add_modal();
            var clone = bank.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, title, ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(bank);
                _ = CheckWriterHelper.AddBankAsync(bank);
                if (!isEdit)
                    bankAccountList.Add(new BankAccountPair(bank));
                SetBankAccountHierarchy();
                bankAccountView.Refresh();
            }
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var isEdit = true;
            var title = "Edit Account";
            var pair = (sender as FrameworkElement).DataContext as BankAccountPair;
            var account = pair?.Account;
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
                    bankAccountList.Add(new BankAccountPair(account));
                SetBankAccountHierarchy();
                bankAccountView.Refresh();
            }
        }

        private void WriteCheck_Click(object sender, RoutedEventArgs e)
        {
            bool isEdit = true;
            string title = "Edit Check";
            var check = (sender as FrameworkElement).DataContext as CheckWriterCheckViewModel;
            if (check == null)
            {
                isEdit = false;
                title = "Write Check";
                check = new CheckWriterCheckViewModel();
                check.Account = ((sender as FrameworkElement).DataContext as BankAccountPair)?.Account;
            }
            var modal = new check_add_modal();
            var clone = check.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, title, ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(check);
                _ = CheckWriterHelper.AddCheckAsync(check);
                if (!isEdit)
                    checkList.Add(check);
                checkView.Refresh();
            }
        }

        private void PrintCheck_Click(object sender, RoutedEventArgs e)
        {
            var check = (sender as FrameworkElement).DataContext as CheckWriterCheckViewModel;
            Action printedAction = () =>
            {
                check.PrintedDate = DateTime.Now;
                _ = CheckWriterHelper.AddCheckAsync(check);
                checkView.Refresh();
            };
            Reports.report_form.IsPrinted += printedAction;
            Reports.ReportsHelper.PrintBankCheck(check);
            Reports.report_form.IsPrinted -= printedAction;
        }

        private void ResetCompanySelection_Click(object sender, RoutedEventArgs e)
        {
            Selections.SelectedBankAccountPair = null;
        }
    }

    public class BankAccountTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BankTemplate { get; set; }
        public DataTemplate AccountTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = container as ContentPresenter;
            DataGridCell cell = presenter.Parent as DataGridCell;
            var pair = cell.DataContext as check_writer_page.BankAccountPair;
            if (pair != null)
            {
                if (pair.IsBank) return BankTemplate;
                else return AccountTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
