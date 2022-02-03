using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;
using bolt5.CloneCopy;
using Petty_Cash.Sub_Modals;
using Petty_Cash.Objects;
using System.ComponentModel;
using System.Windows.Data;
using Petty_Cash.Classes;
using System.Collections.ObjectModel;
using Xceed.Wpf.Toolkit;
using PropertyChanged;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for voucher_list_page.xaml
    /// </summary>
    public partial class voucher_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public int FilterPayee { get; set; }
            public string FilterVoucherNumber { get; set; }
            public bool ShowOnlyOpenVouchers { get; set; } = true;
            public bool ShowReplenishVouchers { get; set; }
            public DateTime? FilterDateFrom { get; set; }
            public DateTime? FilterDateTo { get; set; }

            public ObservableCollection<PayeeViewModel> PayeeList { get; set; } = new ObservableCollection<PayeeViewModel>();

            [DoNotNotify]
            public bool CanFilter { get; set; } = true;

            public void Reset()
            {
                CanFilter = false;
                FilterPayee = 0;
                FilterVoucherNumber = string.Empty;
                FilterDateFrom = null;
                FilterDateTo = null;
                ShowOnlyOpenVouchers = true;
                ShowReplenishVouchers = false;
                CanFilter = true;
            }
        }

        public class TotalGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            [AlsoNotifyFor("BalanceTotal")]
            public decimal ReplenishTotal { get; set; }
            [AlsoNotifyFor("BalanceTotal")]
            public decimal ExpenseTotal { get; set; }
            [AlsoNotifyFor("BalanceTotal")]
            public decimal ReturnTotal { get; set; }
            public decimal BalanceTotal { get { return ReplenishTotal - (ExpenseTotal - ReturnTotal); } }

            public decimal TotalReplenish { get; set; }
            public decimal TotalVoucher { get; set; }
            public decimal TotalExpense { get; set; }
        }

        public class VoucherDetails : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public VoucherViewModel Voucher { get; set; }
            public List<PettyCashViewModel> Transactions { get; set; } = new List<PettyCashViewModel>();

            public VoucherDetails(VoucherViewModel voucher)
            {
                this.Voucher = voucher;
                //load transactions in task
                var transactionsTask = PettyCashContextHelper.GetPettyCashListFromVoucherIdAsync(this.Voucher.Id);
                transactionsTask.ContinueWith(t => Transactions.AddRange(t.Result));
            }
        }

        public static readonly RoutedEvent LiquidateVoucherEvent = EventManager.RegisterRoutedEvent(nameof(LiquidateVoucher), RoutingStrategy.Bubble, typeof(LiquidateVoucherEventHandler), typeof(voucher_list_page));
        public event LiquidateVoucherEventHandler LiquidateVoucher
        {
            add { AddHandler(LiquidateVoucherEvent, value); }
            remove { RemoveHandler(LiquidateVoucherEvent, value); }
        }

        private ICollectionView voucherItemList;
        private List<VoucherViewModel> voucherList = new List<VoucherViewModel>();

        private FilterGroup filters;
        private TotalGroup totals;

        public voucher_list_page()
        {
            InitializeComponent();
            voucherItemList = new CollectionViewSource() { Source = voucherList }.View;
            voucherItemList.Filter = x => DoFilterVoucher(x as VoucherViewModel);
            vouchersDg.ItemsSource = voucherItemList;
            voucherItemList.SortDescriptions.Add(new SortDescription("VoucherDate", ListSortDirection.Descending));

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;

            totals = new TotalGroup();
            totalsGrid.DataContext = totals;
            totalsGroup.DataContext = totals;

            for (int i = 0; i < vouchersDg.Columns.Count; i++)
            {
                Binding widthBinding = new Binding("Columns[" + i + "].ActualWidth");
                widthBinding.ElementName = "vouchersDg";
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.SetBinding(ColumnDefinition.WidthProperty, widthBinding);
                totalsGrid.ColumnDefinitions.Add(columnDefinition);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            voucherList.Clear();
            voucherList.AddRange(GetVoucherList());
            voucherItemList.Refresh();

            SetFilters();
            CalculateTotalsOnce();
            CalculateTotals();
        }

        private IEnumerable<VoucherViewModel> GetVoucherList()
        {
            var task = PettyCashContextHelper.GetVoucherListAsync();
            task.Wait();
            return task.Result;
        }

        private bool DoFilterVoucher(VoucherViewModel i)
        {
            bool flag = true;

            //payee
            if (filters.FilterPayee != 0)
                flag &= i.Payee != null && i.Payee.Id == filters.FilterPayee;

            //voucher number
            if (!string.IsNullOrWhiteSpace(filters.FilterVoucherNumber))
                flag &= i.VoucherNumber.IndexOf(filters.FilterVoucherNumber.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) != -1;

            //open vouchers
            if (filters.ShowOnlyOpenVouchers)
                flag &= !i.IsLiquidated;

            //replenish vouchers
            if (!filters.ShowReplenishVouchers)
                flag &= i.TransactionType != TransactionType.Replenish;

            //date range
            if (filters.FilterDateFrom != null && filters.FilterDateTo != null)
                flag &= i.VoucherDate.Date >= filters.FilterDateFrom.Value.Date && i.VoucherDate.Date <= filters.FilterDateTo.Value.Date;

            return flag;
        }

        private void ApplyDateRange_btn_Click(object sender, RoutedEventArgs e)
        {
            DateRangeType? dateRange = (sender as FrameworkElement).Tag as DateRangeType?;
            if (dateRange != null)
            {
                DateTime start, end;
                DateRangeHelper.GetDateRange(dateRange.Value, out start, out end);
                filters.CanFilter = false;
                filters.FilterDateFrom = start;
                filters.FilterDateTo = end;
                filters.CanFilter = true;
            }
            Filters_PropertyChanged(null, null);
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (filters.CanFilter)
            {
                voucherItemList.Refresh();
                CalculateTotals();
            }
        }

        private void SetFilters()
        {
            filters.PayeeList.Clear();
            var payeeTask = PettyCashContextHelper.GetPayeeListAsync(false);
            Task.WaitAll(payeeTask);

            //foreach (var p in payeeTask.Result.Where(i => i.Id != -2)) //exclude admin expense payee
            //{
            //    var list = voucherList.Where(i => i.Payee.Id == p.Id && !i.IsLiquidated); //get all open vouchers from this payee
            //    if (list != null && list.Count() != 0)
            //        p.Amount = list.Sum(i => (i.AmountReceived ?? 0));
            //    filters.PayeeList.Add(p);
            //}
            filters.PayeeList = new ObservableCollection<PayeeViewModel>(payeeTask.Result);
            _ = PettyCashContextHelper.GetPayeeBalanceAsync(filters.PayeeList);
            filters.PayeeList.Insert(0, new PayeeViewModel() { PayeeName = "-" });
            voucherItemList.Refresh();
        }

        private void CalculateTotals()
        {
            var filteredList = voucherItemList.OfType<VoucherViewModel>();
            totals.TotalVoucher = filteredList.Sum(i => (i.AmountReceived ?? 0));
            totals.TotalReplenish = filteredList.Sum(i => (i.AmountReplenished ?? 0));
            totals.TotalExpense = filteredList.Where(i => (i.Payee?.Id ?? 0) != -2).Sum(i => i.ExpenseAmount); //exlude admin payee from calculation
        }

        private void CalculateTotalsOnce()
        {
            Task.Run(async () =>
            {
                var list = await PettyCashContextHelper.GetPettyCashAmountListAsync();
                totals.ReplenishTotal = list.Where(i => i.TransactionType == TransactionType.Replenish).Sum(i => i.AmountIn ?? 0);
                totals.ExpenseTotal = list.Where(i => i.TransactionType == TransactionType.Expense && (i.Payee?.Id ?? 0) != -2).Sum(i => i.AmountOut ?? 0); //exclude admin payee from calculation
                totals.ReturnTotal = list.Sum(i => i.AmountReturn ?? 0);
            });
        }

        private void AddVoucher_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new voucherAddEdit_modal();
            var voucherVm = new VoucherViewModel();
            voucherVm.VoucherDate = DateTime.Now.Date;
            modal.DataContext = voucherVm;
            if (ModalForm.ShowModal(modal, "Add Voucher", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _ = PettyCashContextHelper.AddUpdateVoucherAsync(voucherVm);
                voucherList.Add(voucherVm);
                voucherItemList.Refresh();
            }
        }

        private void ReplenishPettyCash_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new replenishAdd_modal();
            var voucherVm = new VoucherViewModel();
            voucherVm.VoucherDate = DateTime.Now.Date;
            modal.DataContext = voucherVm;
            if (ModalForm.ShowModal(modal, "Replenish Petty Cash", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                voucherVm.AmountReplenished = voucherVm.Check?.Amount;
                _ = PettyCashContextHelper.AddUpdateVoucherAsync(voucherVm);
                voucherList.Add(voucherVm);
                filters.ShowReplenishVouchers = true;
                voucherItemList.Refresh();
                //set check as used
                if (voucherVm.Check != null)
                    _ = CheckWriterHelper.SetCheckAsUsed(voucherVm.Check);
            }
        }

        private void EditVoucher_btn_Click(object sender, RoutedEventArgs e)
        {
            var voucherVm = (sender as FrameworkElement).DataContext as VoucherViewModel;
            UserControl modal = null;
            string title = "";
            if (voucherVm.TransactionType == TransactionType.Replenish)
            {
                modal = new replenishAdd_modal();
                title = "Edit Replenish";
            }
            else if (voucherVm.TransactionType == TransactionType.Expense)
            {
                modal = new voucherAddEdit_modal();
                title = "Edit Voucher";
            }
            var clone = voucherVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, title, ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(voucherVm);
                if (voucherVm.TransactionType == TransactionType.Replenish)
                    voucherVm.AmountReplenished = voucherVm.Check?.Amount;
                _ = PettyCashContextHelper.AddUpdateVoucherAsync(voucherVm);
            }
        }

        private void LiquidateVoucher_btn_Click(object sender, RoutedEventArgs e)
        {
            var voucherVm = (sender as FrameworkElement).DataContext as VoucherViewModel;
            RaiseEvent(new LiquidateVoucherEventArgs(LiquidateVoucherEvent, voucherVm));
        }

        private void CancelVoucher_btn_Click(object sender, RoutedEventArgs e)
        {
            var voucher = (sender as FrameworkElement).DataContext as VoucherViewModel;
            if (voucher.TransactionType == TransactionType.Expense)
                CancelExpenseVoucher(voucher);
            else if (voucher.TransactionType == TransactionType.Replenish)
                CancelReplenishVoucher(voucher);
        }

        private void CancelReplenishVoucher(VoucherViewModel voucher)
        {
            //enter admin password to cancel voucher
            var modal = new adminPassword_modal();
            if (ModalForm.ShowModal(modal, "Cancel Replenish?", ModalButtons.YesNo) == ModalResult.Yes)
            {
                voucher.AmountReplenished = 0;
                _ = PettyCashContextHelper.AddUpdateVoucherAsync(voucher);
                System.Windows.MessageBox.Show("Replenish is Cancelled", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelExpenseVoucher(VoucherViewModel voucher)
        {
            //enter admin password to cancel voucher
            var modal = new adminPassword_modal();
            if (ModalForm.ShowModal(modal, string.Format("Cancel Voucher # {0}?", voucher.VoucherNumber), ModalButtons.YesNo) == ModalResult.Yes)
            {
                _ = PettyCashContextHelper.ClearVoucherTransaction(voucher);
                System.Windows.MessageBox.Show(string.Format("Voucher # {0} is Cancelled", voucher.VoucherNumber), "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowVoucherDetails_btn_Click(object sender, RoutedEventArgs e)
        {
            var voucherVm = (sender as FrameworkElement).DataContext as VoucherViewModel;
            voucherDetailsPopup.DataContext = new VoucherDetails(voucherVm);
            voucherDetailsPopup.PlacementTarget = sender as UIElement;
            voucherDetailsPopup.IsOpen = true;
        }

        private void ResetFilters_btn_Click(object sender, RoutedEventArgs e)
        {
            filters.Reset();
            Filters_PropertyChanged(null, null);
        }
    }

    public delegate void LiquidateVoucherEventHandler(object sender, LiquidateVoucherEventArgs e);
    public class LiquidateVoucherEventArgs : RoutedEventArgs
    {
        public VoucherViewModel Voucher { get; private set; }

        public LiquidateVoucherEventArgs(RoutedEvent routedEvent, VoucherViewModel voucher) : base(routedEvent)
        {
            this.Voucher = voucher;
        }
    }
}
