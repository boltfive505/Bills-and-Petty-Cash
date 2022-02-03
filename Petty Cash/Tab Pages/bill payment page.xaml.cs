using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Petty_Cash.Classes;
using Petty_Cash.Objects.Bills;
using Petty_Cash.Sub_Modals;
using bolt5.ModalWpf;
using bolt5.CloneCopy;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using PropertyChanged;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for bill_payment_page.xaml
    /// </summary>
    public partial class bill_payment_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public int FilterBill { get; set; }
            public int FilterCategory { get; set; }
            public string FilterBillerName { get; set; }
            //public int? FilterDueMonth { get; set; }
            //public int? FilterDueYear { get; set; }
            public DateTime? FilterDateFrom { get; set; }
            public DateTime? FilterDateTo { get; set; }
            public bool ShowPaidBills { get; set; }
            [DoNotNotify]
            public bool CanRefresh { get; set; } = true;

            public ICollectionView ItemBillNameList
            {
                get { return _itemBillNameListSource?.View; }
            }

            public List<BillPeriodViewModel> CategoryBillNameList { get; set; } = new List<BillPeriodViewModel>();
            public ObservableCollection<string> BillerNameList { get; set; } = new ObservableCollection<string>();
            public ObservableCollection<Objects.CategoryViewModel> CategoryNameList { get; set; } = new ObservableCollection<Objects.CategoryViewModel>();

            private CollectionViewSource _itemBillNameListSource;

            public FilterGroup()
            {
                _itemBillNameListSource = new CollectionViewSource() { Source = CategoryBillNameList };
                ItemBillNameList.Filter = x => DoFilterCategoryBillName(x as BillPeriodViewModel);

                //DateTime now = DateTime.Now;
                //FilterDueMonth = now.Month;
                //FilterDueYear = now.Year;
            }

            private bool DoFilterCategoryBillName(BillPeriodViewModel i)
            {
                bool flag = true;
                if (FilterCategory != 0 && i.Category != null)
                {
                    flag &= (i.Category != null && i.Category.Id == FilterCategory) || i.Id == 0;
                }
                return flag;
            }

            private void OnFilterCategoryChanged()
            {
                ItemBillNameList.Refresh();
                FilterBill = 0;
            }

            public void Reset()
            {
                CanRefresh = false;
                FilterBill = 0;
                FilterCategory = 0;
                FilterBillerName = null;
                FilterDateFrom = null;
                FilterDateTo = null;
                ShowPaidBills = false;
                //DateTime now = DateTime.Now;
                //FilterDueMonth = now.Month;
                //FilterDueYear = now.Year;
                CanRefresh = true;
            }
        }

        public class TotalsGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public decimal TotalBill { get; set; }
            public decimal TotalPaid { get; set; }
            public decimal TotalWht { get; set; }
            public decimal TotalVat { get; set; }
            public decimal TotalNet { get; set; }

            public void Reset()
            {
                TotalBill = 0;
                TotalPaid = 0;
                TotalWht = 0;
                TotalVat = 0;
                TotalNet = 0;
            }
        }

        private ICollectionView paymentItemList;
        private List<BillPaymentViewModel> paymentList = new List<BillPaymentViewModel>();
        private FilterGroup filters;
        private TotalsGroup totals;
        private int selectBill = 0;

        public bill_payment_page()
        {
            InitializeComponent();
            paymentItemList = new CollectionViewSource() { Source = paymentList }.View;
            paymentItemList.SortDescriptions.Add(new SortDescription("PeriodDate", ListSortDirection.Descending));
            paymentItemList.Filter = x => DoFilterPayment(x as BillPaymentViewModel);
            paymentDg.ItemsSource = paymentItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;

            totals = new TotalsGroup();
            totalsGrid.DataContext = totals;

            for (int i = 0; i < paymentDg.Columns.Count; i++)
            {
                Binding widthBinding = new Binding("Columns[" + i + "].ActualWidth");
                widthBinding.ElementName = "paymentDg";
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.SetBinding(ColumnDefinition.WidthProperty, widthBinding);
                totalsGrid.ColumnDefinitions.Add(columnDefinition);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            paymentList.Clear();
            paymentList.AddRange(GetPaymentList());
            paymentItemList.Refresh();

            RefreshFilterList();
            RefreshTotals();
            _ = GetNumberOfPendingPayment();
            CheckIfHasBillSelection();
        }

        private IEnumerable<BillPaymentViewModel> GetPaymentList()
        {
            var task = BillsPeriodContextHelper.GetBillPaymentListAsync();
            task.Wait();
            return task.Result;
        }

        private async Task GetNumberOfPendingPayment()
        {
            await Task.Run(() =>
            {
                var periodBillPendingCountGroup = paymentList.GroupBy(i => i.Period).Select(g => new
                {
                    Period = g.Key,
                    Count = g.Count(i => i.Amount == 0 || string.IsNullOrEmpty(i.ReceiptFileName))
                });
                paymentList.ForEach(i =>
                {
                    int count = (periodBillPendingCountGroup.FirstOrDefault(x => x.Period.Equals(i.Period))?.Count ?? 0);
                    i.Period.NumberOfPendingPayment = count;
                });
            }).ConfigureAwait(true);
        }

        private void RefreshTotals()
        {
            totals.Reset();
            var filteredPaymentList = paymentItemList.OfType<BillPaymentViewModel>();
            totals.TotalBill = filteredPaymentList.Sum(i => i.BillAmount);
            totals.TotalPaid = filteredPaymentList.Sum(i => (i.Amount ?? 0));
            totals.TotalWht = filteredPaymentList.Where(i => i.Period.TaxType == BillTaxType.Withholding_Tax).Sum(i => i.VatAmount ?? 0);
            totals.TotalVat = filteredPaymentList.Where(i => i.Period.TaxType == BillTaxType.Vat).Sum(i => i.VatAmount ?? 0);
            totals.TotalNet = filteredPaymentList.Where(i => i.Period.TaxType != BillTaxType.Not_Applicable).Sum(i => i.NetVatAmount ?? 0);
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (filters.CanRefresh)
                paymentItemList.Refresh();
            RefreshTotals();
        }

        private void RefreshFilterList()
        {
            //filters.Reset();
            filters.CategoryBillNameList.Clear();
            filters.CategoryNameList.Clear();
            paymentItemList.Refresh();

            var categoryTask = BillsPeriodContextHelper.GetCategoryListAsync();
            var periodTask = BillsPeriodContextHelper.GetBillPeriodSimpleListAsync();
            Task.WaitAll(categoryTask, periodTask);
            //category
            filters.CategoryNameList = new ObservableCollection<Objects.CategoryViewModel>(categoryTask.Result.Where(i=> i.IsActive));
            filters.CategoryNameList.Insert(0, new Objects.CategoryViewModel());
            //biller name
            RefreshBillerNameFilterList();
            //bill
            filters.CategoryBillNameList.AddRange(periodTask.Result.Where(i => i.IsActive && i.Category.IsActive));
            filters.CategoryBillNameList.Insert(0, new BillPeriodViewModel());
            try
            {
                filters.ItemBillNameList.Refresh();
            }
            catch
            { }
        }

        private void RefreshBillerNameFilterList()
        {
            filters.BillerNameList.Clear();
            filters.BillerNameList = new ObservableCollection<string>(paymentList.GroupBy(i => i.Period).Where(g => g.Key.IsActive && g.Key.Category.IsActive)
                                                                                    .Select(g => g.Key.BillerName.Trim())
                                                                                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                                                                    .OrderBy(x => x));
            filters.BillerNameList.Insert(0, "");
        }

        private bool DoFilterPayment(BillPaymentViewModel i)
        {
            bool flag = true;

            if (!i.Period.IsActive || !i.Period.Category.IsActive) return false;

            //category
            if (filters.FilterCategory != 0)
                flag &= i.Period.Category.Id == filters.FilterCategory;

            //bill
            if (filters.FilterBill != 0)
                flag &= i.Period.Id == filters.FilterBill;

            //biller name
            if (!string.IsNullOrWhiteSpace(filters.FilterBillerName))
            {
                flag &= StringComparer.InvariantCultureIgnoreCase.Equals(i.Period.BillerName.Trim(), filters.FilterBillerName);
            }

            //filter by due month and year
            //if (filters.FilterDueMonth != null && filters.FilterDueMonth != 0)
            //    flag &= i.PeriodDate.Month == filters.FilterDueMonth;
            //if (filters.FilterDueYear != null)
            //    flag &= i.PeriodDate.Year == filters.FilterDueYear;

            //filter by from and to range
            if (filters.FilterDateFrom != null && filters.FilterDateTo != null)
                flag &= i.PeriodDate.Date >= filters.FilterDateFrom.Value.Date && i.PeriodDate.Date <= filters.FilterDateTo.Value.Date;

            //show paid bills
            if (!filters.ShowPaidBills)
                flag &= (i.Amount ?? 0) == 0;

            return flag;
        }

        private void EditPayment_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new billPaymentAddEdit_modal();
            var paymentVm = (sender as FrameworkElement).DataContext as BillPaymentViewModel;
            var clone = paymentVm.DeepClone();
            modal.DataContext = clone;
            ModalResult result = ModalForm.ShowModal(modal, "Edit Bill Payment", ModalButtons.SaveCancelDelete);
            if (result == ModalResult.Save)
            {
                clone.DeepCopyTo(paymentVm);
                //if (paymentVm.PaymentDate != null && (paymentVm.Amount ?? 0) > 0)
                //    paymentVm.PaymentDate = DateTime.Now.Date;
                _ = BillsPeriodContextHelper.AddUpdateBillPaymentAsync(paymentVm);
                paymentItemList.Refresh();
                _ = GetNumberOfPendingPayment();
            }
            else if (result == ModalResult.Delete)
            {
                _ = BillsPeriodContextHelper.DeleteBillPaymentAsync(paymentVm);
                paymentList.Remove(paymentVm);
                paymentItemList.Refresh();
                _ = GetNumberOfPendingPayment();
            }
        }

        private void PayBill_Click(object sender, RoutedEventArgs e)
        {
            //_ = GetNumberOfPendingPayment();
            var modal = new billPaymentPayOnly_modal();
            var paymentVm = (sender as FrameworkElement).DataContext as BillPaymentViewModel;
            var clone = paymentVm.DeepClone();
            modal.DataContext = clone;
            ModalResult result = ModalForm.ShowModal(modal, "Pay Bill", ModalButtons.SaveCancel);
            if (result == ModalResult.Save)
            {
                clone.DeepCopyTo(paymentVm);
                if ((paymentVm.Amount ?? 0) > 0)
                    paymentVm.PaymentDate = DateTime.Now.Date;
                _ = BillsPeriodContextHelper.AddUpdateBillPaymentAsync(paymentVm);
                paymentItemList.Refresh();
                _ = GetNumberOfPendingPayment();
                //set check as used
                if (paymentVm.Check != null)
                {
                    _ = CheckWriterHelper.SetCheckAsUsed(paymentVm.Check);
                    paymentVm.Check = null;
                }
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
                MessageBox.Show("ERROR: File not found", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool BillName_cbox_SearchText(object item, string searchText)
        {
            var i = item as BillPeriodViewModel;
            if (i.BillerName != null)
            {
                string keyword = searchText.Trim();
                return i.BillerName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                    i.BillDescription.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            return true;
        }

        public void SelectBill(BillPeriodViewModel bill)
        {
            selectBill = bill.Id;
        }

        private void CheckIfHasBillSelection()
        {
            if (selectBill != 0)
            {
                filters.CanRefresh = false;
                filters.FilterCategory = 0;
                filters.FilterBill = selectBill;
                //filters.FilterDueMonth = 0;
                //filters.FilterDueYear = DateTime.Now.Year;
                filters.ShowPaidBills = true;

                DateTime now = DateTime.Now;
                filters.FilterDateFrom = new DateTime(now.Year, 1, 1);
                filters.FilterDateTo = now;
                filters.CanRefresh = true;
                selectBill = 0;

                paymentItemList.Refresh();
            }
        }

        private void ExportExcel_btn_Click(object sender, RoutedEventArgs e)
        {
            var list = paymentItemList.OfType<BillPaymentViewModel>();
            ExcelHelper.ExportBillPaymentList(list);
        }

        private void ApplyDateRange_btn_Click(object sender, RoutedEventArgs e)
        {
            filters.CanRefresh = false;
            DateRangeType? dateRange = (sender as FrameworkElement).Tag as DateRangeType?;
            if (dateRange != null)
            {
                DateTime start, end;
                DateRangeHelper.GetDateRange(dateRange.Value, out start, out end);
                filters.FilterDateFrom = start;
                filters.FilterDateTo = end;
            }
            filters.CanRefresh = true;
            Filters_PropertyChanged(null, null);
        }

        private void ResetFilters_btn_Click(object sender, RoutedEventArgs e)
        {
            filters.Reset();
            Filters_PropertyChanged(null, null);
        }

        private void SortOpenBill_Click(object sender, RoutedEventArgs e)
        {
            paymentItemList.SortDescriptions.Clear();
            paymentItemList.SortDescriptions.Add(new SortDescription("Period.BillerName", ListSortDirection.Ascending));
            paymentItemList.SortDescriptions.Add(new SortDescription("Period.BillDescription", ListSortDirection.Ascending));
        }

        private void BillerInfo_btn_Click(object sender, RoutedEventArgs e)
        {
            var periodVm = ((sender as FrameworkElement).DataContext as BillPaymentViewModel)?.Period;
            billerInfoPopup.DataContext = periodVm;
            billerInfoPopup.PlacementTarget = sender as FrameworkElement;
            billerInfoPopup.IsOpen = true;
        }
    }
}
