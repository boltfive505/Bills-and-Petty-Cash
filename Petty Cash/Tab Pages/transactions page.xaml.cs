using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.IO;
using Petty_Cash.Sub_Modals;
using Petty_Cash.Model;
using Petty_Cash.Objects;
using Petty_Cash.Classes;
using bolt5.ModalWpf;
using bolt5.CloneCopy;
using System.ComponentModel;
using PropertyChanged;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for transactions_page.xaml
    /// </summary>
    public partial class transactions_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public int FilterCategory { get; set; }
            public int FilterPayee { get; set; }
            public string FilterVoucherNumber { get; set; }
            public TransactionType FilterTransaction { get; set; }
            public DateTime? FilterDateFrom { get; set; }
            public DateTime? FilterDateTo { get; set; }

            public ObservableCollection<CategoryViewModel> CategoryList { get; set; } = new ObservableCollection<CategoryViewModel>();
            public ObservableCollection<PayeeViewModel> PayeeList { get; set; } = new ObservableCollection<PayeeViewModel>();
            [DoNotNotify]
            public bool CanFilter { get; set; } = true;

            public void Reset()
            {
                CanFilter = false;
                FilterCategory = 0;
                FilterPayee = 0;
                FilterVoucherNumber = string.Empty;
                FilterTransaction = TransactionType.Any;
                FilterDateFrom = null;
                FilterDateTo = null;
                CanFilter = true;
            }
        }

        public class BalanceGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public decimal ExpenseTotal { get; set; }
            [AlsoNotifyFor("GrandTotalExpense")]
            public decimal ReturnTotal { get; set; }

            [AlsoNotifyFor("BalanceTotalOnce")]
            public decimal ReplenishTotalOnce { get; set; }
            [AlsoNotifyFor("BalanceTotalOnce")]
            public decimal ExpenseTotalOnce { get; set; }
            [AlsoNotifyFor("BalanceTotalOnce")]
            public decimal ReturnTotalOnce { get; set; }
            public decimal BalanceTotalOnce { get { return ReplenishTotalOnce - (ExpenseTotalOnce - ReturnTotalOnce); } }

            public decimal VatTotal { get; set; }
            public decimal NetVatTotal { get; set; }
            public decimal NonVatTotal { get; set; }

            [AlsoNotifyFor("GrandTotalExpense")]
            public decimal AdminExpenseTotal { get; set; }
            public decimal GrandTotalExpense { get { return ExpenseTotal + AdminExpenseTotal; } }
        }

        private ICollectionView pettyItemList;
        private List<PettyCashViewModel> pettyList = new List<PettyCashViewModel>();
        private FilterGroup filters;
        private BalanceGroup balances;

        public transactions_page()
        {
            InitializeComponent();
            pettyItemList = new CollectionViewSource() { Source = pettyList }.View;
            pettyItemList.Filter = x => DoFilterPettyCash(x as PettyCashViewModel);
            //pettyItemList.SortDescriptions.Add(new SortDescription("Voucher.VoucherNumber", ListSortDirection.Descending));
            pettyItemList.SortDescriptions.Add(new SortDescription("TransactionDate", ListSortDirection.Descending));
            pettyDg.ItemsSource = pettyItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;

            for (int i = 0; i < pettyDg.Columns.Count; i++)
            {
                Binding columnBinding = new Binding("Columns[" + i + "].ActualWidth");
                columnBinding.ElementName = "pettyDg";
                ColumnDefinition column = new ColumnDefinition();
                column.SetBinding(ColumnDefinition.WidthProperty, columnBinding);
                totalsGrid.ColumnDefinitions.Add(column);
            }

            balances = new BalanceGroup();
            totalsGrid.DataContext = balances;
            totalsGroup.DataContext = balances;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            pettyList.Clear();
            pettyList.AddRange(GetPettyCashList());
            pettyItemList.Refresh();

            RecalculateBalanceOnlyOnce();
            RecalculateBalance();
            SetupFilterList();
        }

        private IEnumerable<PettyCashViewModel> GetPettyCashList()
        {
            var task = PettyCashContextHelper.GetPettyCashListAsync();
            task.Wait();
            return task.Result;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (filters.CanFilter)
            {
                pettyItemList.Refresh();
                RecalculateBalance();
            }
        }

        private bool DoFilterPettyCash(PettyCashViewModel i)
        {
            bool flag = true;

            //transaction
            if (filters.FilterTransaction != TransactionType.Any)
                flag &= i.TransactionType == filters.FilterTransaction;

            //category
            if (filters.FilterCategory != 0)
                flag &= i.Category != null && i.Category.Id == filters.FilterCategory;

            //payee
            if (filters.FilterPayee != 0)
                flag &= i.Payee != null && i.Payee.Id == filters.FilterPayee;

            //voucher number
            if (!string.IsNullOrWhiteSpace(filters.FilterVoucherNumber))
                flag &= (i.Voucher?.VoucherNumber ?? "").IndexOf(filters.FilterVoucherNumber.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) != -1;

            //date range
            if (filters.FilterDateFrom != null && filters.FilterDateTo != null)
                flag &= i.TransactionDate.Date >= filters.FilterDateFrom.Value.Date &&  i.TransactionDate.Date <= filters.FilterDateTo.Value.Date;

            return flag;
        }

        private void SetupFilterList()
        {
            filters.CategoryList.Clear();
            filters.PayeeList.Clear();
            filters.Reset();

            var categoryTask = Classes.PettyCashContextHelper.GetCategoryListAsync();
            var payeeTask = Classes.PettyCashContextHelper.GetPayeeListAsync(false);
            Task.WaitAll(categoryTask, payeeTask);

            filters.CategoryList = new ObservableCollection<CategoryViewModel>(categoryTask.Result);
            filters.CategoryList.Insert(0, new CategoryViewModel());
            filters.PayeeList = new ObservableCollection<PayeeViewModel>(payeeTask.Result);
            filters.PayeeList.Insert(0, new PayeeViewModel());

            pettyItemList.Refresh();
        }

        private void ResetFilters_btn_Click(object sender, RoutedEventArgs e)
        {
            filters.Reset();
            Filters_PropertyChanged(null, null);
        }

        private void ApplyDateRange_btn_Click(object sender, RoutedEventArgs e)
        {
            filters.CanFilter = false;
            DateRangeType? dateRange = (sender as FrameworkElement).Tag as DateRangeType?;
            if (dateRange != null)
            {
                DateTime start, end;
                DateRangeHelper.GetDateRange(dateRange.Value, out start, out end);
                filters.FilterDateFrom = start;
                filters.FilterDateTo = end;
            }
            filters.CanFilter = true;
            Filters_PropertyChanged(null, null);
        }

        private void RecalculateBalance()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                //get filtered petty cash list
                var list = pettyItemList.OfType<PettyCashViewModel>();
                //balances.ReplenishTotal = list.Where(i => i.TransactionType == TransactionType.Replenish).Sum(i => i.AmountIn ?? 0);
                balances.ExpenseTotal = list.Where(i => i.TransactionType == TransactionType.Expense && i.Payee.Id != -2).Sum(i => i.AmountOut ?? 0); //exclude admin payee on calculate balance
                balances.ReturnTotal = list.Sum(i => i.AmountReturn ?? 0);

                balances.VatTotal = list.Where(i => i.IsVat).Sum(i => i.VatAmount ?? 0);
                balances.NetVatTotal = list.Where(i => i.IsVat).Sum(i => i.NetVatAmount ?? 0);
                balances.NonVatTotal = list.Where(i => !i.IsVat).Sum(i => i.NonVatAmount ?? 0);

                balances.AdminExpenseTotal = list.Where(i => i.TransactionType == TransactionType.Expense && i.Payee.Id == -2).Sum(i => i.AmountOut ?? 0);

                balances.ExpenseTotalOnce = pettyList.Where(i => i.TransactionType == TransactionType.Expense && i.Payee.Id != -2).Sum(i => i.AmountOut ?? 0); //exclude admin payee on calculate balance
                balances.ReturnTotalOnce = pettyList.Sum(i => i.AmountReturn ?? 0);
            });
        }

        private void RecalculateBalanceOnlyOnce()
        {
            Task.Run(async () =>
            {
                //get replenish total from vouchers
                var list = await PettyCashContextHelper.GetReplenishVoucherAsync();
                balances.ReplenishTotalOnce = list.Sum(i => i.AmountReplenished ?? 0);
            });
        }

        private void AddPettyCash_btn_Click(object sender, RoutedEventArgs e)
        {
            AddPettyCash(null);
        }

        public void AddPettyCash(VoucherViewModel voucherVm)
        {
            var pettyCashVm = new PettyCashViewModel();
            //pre-select voucher, if available
            pettyCashVm.Payee = voucherVm?.Payee;
            pettyCashVm.Voucher = voucherVm;

            while (true)
            {
                bool closeModal = false;
                var modal = new pettyCashAddEdit_modal();
                pettyCashVm.TransactionDate = DateTime.Now;
                modal.DataContext = pettyCashVm;
                ModalResult result;
                object key;
                ModalForm.ShowCustomModal(modal, "Add Petty Cash Expense", Application.Current.FindResource("VoucherButtons") as DataTemplate, out result, out key);
                if (result == ModalResult.Save)
                {
                    if (pettyCashVm.AmountOut > 0)
                    {
                        //this will be for scenario, if they want to just close the voucher without adding petty cash
                        _ = PettyCashContextHelper.AddUpdatePettyCashAsync(pettyCashVm, pettyCashVm.RemainingBalance == 0);
                        pettyList.Add(pettyCashVm);
                        pettyItemList.Refresh();
                        RecalculateBalance();
                    }

                    var temp = new PettyCashViewModel();
                    if (key.Equals("CloseVoucher") && pettyCashVm.RemainingBalance > 0)
                    {
                        //close voucher, return balance amount
                        PettyCashViewModel pettyCashCloseVm = new PettyCashViewModel();
                        pettyCashCloseVm.Payee = pettyCashVm.Payee;
                        pettyCashCloseVm.Voucher = pettyCashVm.Voucher;
                        pettyCashCloseVm.AmountReturn = pettyCashVm.RemainingBalance;
                        pettyCashCloseVm.TransactionDate = pettyCashVm.TransactionDate.AddSeconds(1);
                        pettyCashCloseVm.Purpose = "Return Balance for Voucher# " + pettyCashVm.Voucher.VoucherNumber;
                        pettyList.Add(pettyCashCloseVm);
                        pettyItemList.Refresh();
                        RecalculateBalance();
                        _ = PettyCashContextHelper.AddUpdatePettyCashAsync(pettyCashCloseVm, true);
                        closeModal = true;
                    }
                    if (key.Equals("AddMore"))
                    {
                        //duplicate other values, and add again
                        temp.Payee = pettyCashVm.Payee;
                        temp.Category = pettyCashVm.Category;
                        temp.Purpose = pettyCashVm.Purpose;
                        if (pettyCashVm.RemainingBalance > 0) //if still have balance, duplicate voucher
                            temp.Voucher = pettyCashVm.Voucher;
                    }
                    else
                    {
                        closeModal = true;
                    }
                    pettyCashVm = temp.DeepClone();
                }
                else if (result == ModalResult.Cancel && key.Equals("ClearVoucher") && pettyCashVm.Voucher != null)
                {
                    closeModal = true;
                    //delete all transactions for this voucher
                    _ = PettyCashContextHelper.ClearVoucherTransaction(pettyCashVm.Voucher);
                    pettyList.RemoveAll(i => i.Voucher.Id == pettyCashVm.Voucher.Id);
                    pettyItemList.Refresh();
                    RecalculateBalance();
                }
                else
                {
                    closeModal = true;
                }
                if (closeModal)
                    break;
            }
        }

        private void ReplenishPettyCash_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new replenishAdd_modal();
            var pettyCashVm = new PettyCashViewModel();
            pettyCashVm.TransactionDate = DateTime.Now.Date;
            modal.DataContext = pettyCashVm;
            if (ModalForm.ShowModal(modal, "Replenish Petty Cash", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                pettyList.Add(pettyCashVm);
                pettyItemList.Refresh();
                RecalculateBalance();
                _ = PettyCashContextHelper.AddUpdatePettyCashAsync(pettyCashVm, false);
            }
        }

        private void EditTransaction_btn_Click(object sender, RoutedEventArgs e)
        {
            var pettyCashVm = (sender as FrameworkElement).DataContext as PettyCashViewModel;
            UserControl modal = null;
            string title = "";
            ModalButtons buttons = ModalButtons.SaveCancel;
            if (pettyCashVm.TransactionType == TransactionType.Return)
            {
                System.Windows.MessageBox.Show("Cannot edit Cash-Return", "Edit Petty Cash", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (pettyCashVm.TransactionType == TransactionType.Replenish)
            {
                modal = new replenishAdd_modal(true);
                title = "Edit Replenish Petty Cash";
            }
            else if (pettyCashVm.TransactionType == TransactionType.Expense)
            {
                if (pettyCashVm.IsAdminExpense)
                {
                    modal = new pettyCashAdminAddEdit_modal();
                    title = "Edit Admin Expense";
                    buttons = ModalButtons.SaveCancelDelete;
                }
                else
                {
                    modal = new pettyCashEditOnly_modal(true);
                    title = "Edit Petty Cash Expense";
                }
            }
            var clone = pettyCashVm.DeepClone();
            modal.DataContext = clone;
            ModalResult result = ModalForm.ShowModal(modal, title, buttons);
            if (result == ModalResult.Save)
            {
                clone.DeepCopyTo(pettyCashVm);
                _ = PettyCashContextHelper.AddUpdatePettyCashAsync(pettyCashVm, false);
                pettyItemList.Refresh();
                RecalculateBalance();
            }
            else if (result == ModalResult.Delete)
            {
                _ = PettyCashContextHelper.DeletePettyCashAsync(pettyCashVm);
                pettyList.Remove(pettyCashVm);
                pettyItemList.Refresh();
                RecalculateBalance();
            }
        }

        private void OpenPaymentFile_btn_Click(object sender, RoutedEventArgs e)
        {
            string paymentFileName = ((sender as FrameworkElement).DataContext as PettyCashViewModel).PaymentFile;
            string file = FileHelper.GetFile(paymentFileName, PettyCashContextHelper.PAYEMNT_FILE_SUBDIRECTORY);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                System.Windows.MessageBox.Show("ERROR: File not found!");
            }
        }

        private void ExportExcel_btn_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<PettyCashViewModel> list = pettyItemList.OfType<PettyCashViewModel>();
            ExcelHelper.ExportPettyCashList(list);
        }

        private void AddAdminExpense_Click(object sender, RoutedEventArgs e)
        {
            var pettyCash = new PettyCashViewModel();
            pettyCash.Payee = PayeeViewModel.AdminPayee;
            pettyCash.TransactionDate = DateTime.Now;
            var modal = new pettyCashAdminAddEdit_modal();
            modal.DataContext = pettyCash;
            if (ModalForm.ShowModal(modal, "Add Admin Expense", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _ = PettyCashContextHelper.AddUpdatePettyCashAsync(pettyCash, true);
                pettyList.Add(pettyCash);
                pettyItemList.Refresh();
                RecalculateBalance();
            }
        }
    }
}
