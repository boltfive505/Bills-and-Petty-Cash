using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using bolt5.ModalWpf;
using bolt5.CustomMonthlyCalendar;
using bolt5.CloneCopy;
using Petty_Cash.Sub_Modals;
using Petty_Cash.Classes;
using Petty_Cash.Objects.Bills;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using Petty_Cash.Objects;
using Petty_Cash.web_printing;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for bill_calendar_page.xaml
    /// </summary>
    public partial class bill_calendar_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
            public int FilterCategory { get; set; }
            public string FilterBillerName { get; set; }
            public bool FilterInactive { get; set; }

            public bool CanFilter { get; private set; } = true;

            public ObservableCollection<CategoryViewModel> CategoryList { get; set; } = new ObservableCollection<CategoryViewModel>();
            public ObservableCollection<string> BillerNameList { get; set; } = new ObservableCollection<string>();

            public void Reset()
            {
                CanFilter = false;
                FilterKeyword = string.Empty;
                FilterCategory = 0;
                FilterBillerName = null;
                FilterInactive = false;
                CanFilter = true;
            }
        }

        public class FilterCalendarGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public int FilterBill { get; set; }
            public string FilterKeyword { get; set; }

            public ICollectionView ItemBillNameList { get; set; }
            public List<BillPeriodViewModel> CategoryBillNameList { get; set; } = new List<BillPeriodViewModel>();

            public bool CanRefresh { get; set; }

            public FilterCalendarGroup()
            {
                ItemBillNameList = new CollectionViewSource() { Source = CategoryBillNameList }.View;
            }

            public void Reset()
            {
                CanRefresh = false;
                this.FilterBill = 0;
                CanRefresh = true;
            }
        }

        public class PeriodCalendarDisplayCollection : INotifyPropertyChanged, IMonthlyCalendarDayItem
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public DateTime Day { get; set; }
            public IEnumerable<PeriodCalendarDisplay> Items { get; set; }
        }

        public class PeriodCalendarDisplay : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public DateTime PeriodDate { get; set; }
            public BillPeriodViewModel Item { get; set; }
        }

        public static readonly RoutedEvent SelectBillEvent = EventManager.RegisterRoutedEvent(nameof(SelectBill), RoutingStrategy.Bubble, typeof(SelectBillEventHandler), typeof(bill_calendar_page));
        public event SelectBillEventHandler SelectBill
        {
            add { AddHandler(SelectBillEvent, value); }
            remove { RemoveHandler(SelectBillEvent, value); }
        }

        private ICollectionView billPeriodItemList;
        private ICollectionView billCalendarDaysItemList;
        private List<BillPeriodViewModel> billPeriodList = new List<BillPeriodViewModel>();
        private List<PeriodCalendarDisplayCollection> billCalendarList = new List<PeriodCalendarDisplayCollection>();
        private FilterGroup filters;
        private FilterCalendarGroup calendarFilters;

        public bill_calendar_page()
        {
            InitializeComponent();
            billPeriodItemList = new CollectionViewSource() { Source = billPeriodList }.View;
            billPeriodItemList.SortDescriptions.Add(new SortDescription("BillerName", ListSortDirection.Ascending));
            billPeriodItemList.Filter = x => DoFilterPeriod(x as BillPeriodViewModel);
            billPeriodDg.ItemsSource = billPeriodItemList;

            billCalendarDaysItemList = new CollectionViewSource() { Source = billCalendarList }.View;
            billCalendar.ItemsSource = billCalendarDaysItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;

            calendarFilters = new FilterCalendarGroup();
            calendarFilters.PropertyChanged += CalendarFilters_PropertyChanged;
            calendarFiltersGroup.DataContext = calendarFilters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            billPeriodList.Clear();
            billPeriodList.AddRange(GetPeriodList());
            billPeriodItemList.Refresh();

            RefreshPeriodCalendarDisplay();
            RefreshFilterList();
            _ = BillsPeriodContextHelper.SetMissingBillsCountAsync(billPeriodList);
            _ = BillsPeriodContextHelper.SetMissingBillsAmount(billPeriodList);
        }

        private IEnumerable<BillPeriodViewModel> GetPeriodList()
        {
            var task = BillsPeriodContextHelper.GetBillPeriodListAsync();
            task.Wait();
            return task.Result;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (filters.CanFilter)
                billPeriodItemList.Refresh();
        }

        private void CalendarFilters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshPeriodCalendarDisplay();
        }

        private void RefreshFilterList()
        {
            //begin bill period filters
            filters.Reset();
            filters.CategoryList.Clear();

            //category
            var categoryTask = BillsPeriodContextHelper.GetCategoryListAsync();
            categoryTask.Wait();
            filters.CategoryList = new ObservableCollection<CategoryViewModel>(categoryTask.Result);
            filters.CategoryList.Insert(0, new CategoryViewModel());

            //biller name
            RefreshBillerNameFilterList();

            billPeriodItemList.Refresh();
            //end bill period filters

            //begin calendar filters
            calendarFilters.Reset();
            calendarFilters.CategoryBillNameList.Clear();

            var periodTask = BillsPeriodContextHelper.GetBillPeriodSimpleListAsync();
            calendarFilters.CategoryBillNameList.AddRange(periodTask.Result.Where(i => i.IsActive && i.Category.IsActive));
            calendarFilters.CategoryBillNameList.Insert(0, new BillPeriodViewModel());
            //end calendar filters
        }

        private void RefreshBillerNameFilterList()
        {
            filters.BillerNameList.Clear();
            filters.BillerNameList = new ObservableCollection<string>(billPeriodList.Where(i => i.IsActive && i.Category.IsActive)
                                                                                    .Select(i => i.BillerName.Trim())
                                                                                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                                                                    .OrderBy(x => x));
            filters.BillerNameList.Insert(0, "");
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            filters.Reset();
            billPeriodItemList.Refresh();
        }

        private bool DoFilterPeriod(BillPeriodViewModel i)
        {
            bool flag = true;
            
            //category
            if (filters.FilterCategory != 0)
                flag &= i.Category.Id == filters.FilterCategory;
            //keyword
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
            {
                string keyword = filters.FilterKeyword.Trim();
                flag &= i.BillerName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        i.Description.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        (i.AccountName ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        (i.AccountNumber ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        (i.Address ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        (i.UnitNumber ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1 ||
                        (i.TinNumber ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            //biller name
            if (!string.IsNullOrWhiteSpace(filters.FilterBillerName))
            {
                flag &= StringComparer.InvariantCultureIgnoreCase.Equals(i.BillerName.Trim(), filters.FilterBillerName);
            }
            //active
            if (!filters.FilterInactive)
            {
                flag &= i.IsActive && i.Category.IsActive;
            }
            return flag;
        }

        private void AddPeriod_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new billPeriodAddEdit_modal();
            var periodVm = new BillPeriodViewModel();
            modal.DataContext = periodVm;
            if (ModalForm.ShowModal(modal, "Add New Biller", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _ = BillsPeriodContextHelper.AddUpdateBillPeriodAsync(periodVm);
                billPeriodList.Insert(0, periodVm);
                billPeriodItemList.Refresh();
                RefreshPeriodCalendarDisplay();
            }
        }

        private void EditPeriod_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new billPeriodAddEdit_modal();
            var periodVm = (sender as FrameworkElement).DataContext as BillPeriodViewModel;
            var clone = periodVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Bill Period", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(periodVm);
                _ = BillsPeriodContextHelper.AddUpdateBillPeriodAsync(periodVm);
                billPeriodItemList.Refresh();
                RefreshPeriodCalendarDisplay();
            }
        }

        private IEnumerable<PeriodCalendarDisplayCollection> GetPeriodListByDisplayMonth(int year, DateTime startDisplayDate, DateTime endDisplayDate)
        {
            List<PeriodCalendarDisplay> periodDisplays = new List<PeriodCalendarDisplay>();
            var list = billPeriodList.Where(i => i.IsActive);
            //check for filters
            if (!string.IsNullOrWhiteSpace(calendarFilters.FilterKeyword))
            {
                list = list.Where(i =>
                {
                    string keyword = calendarFilters.FilterKeyword.Trim();
                    return i.BillerName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        (i.Description ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        (i.AccountName ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        (i.AccountNumber ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        (i.Address ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        (i.UnitNumber ?? "").IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
                });
            }
            else
            {
                if (calendarFilters.FilterBill != 0)
                {
                    list = list.Where(i => i.Id == calendarFilters.FilterBill);
                }
            }

            foreach (var l in list)
            {
                periodDisplays.AddRange(l.GetPeriodDatesByYear(year)
                    .Where(d => d >= startDisplayDate && d <= endDisplayDate)
                    .Select(d => new PeriodCalendarDisplay()
                    {
                        PeriodDate = d,
                        Item = l
                    }));
            }
            return periodDisplays.GroupBy(i => i.PeriodDate)
                .Select(g => new PeriodCalendarDisplayCollection()
                {
                    Day = g.Key,
                    Items = g
                });
        }

        private void RefreshPeriodCalendarDisplay()
        {
            billCalendarList.Clear();
            billCalendarList.AddRange(GetPeriodListByDisplayMonth(billCalendar.DisplayMonth.Year, billCalendar.DisplayDateStart, billCalendar.DisplayDateEnd));
            billCalendarDaysItemList.Refresh();
        }

        private void billCalendar_DisplayMonthChanged(object sender, EventArgs e)
        {
            RefreshPeriodCalendarDisplay();
        }

        private void AddBillPayment_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new billPaymentAddEdit_modal();
            var periodDisplay = (sender as FrameworkElement).DataContext as PeriodCalendarDisplay; //adding from calendar
            BillPeriodViewModel period = periodDisplay?.Item;
            if (period == null)
                period = (sender as FrameworkElement).DataContext as BillPeriodViewModel; //adding from custom

            var billPaymentVm = new BillPaymentViewModel();
            billPaymentVm.Period = period;
            billPaymentVm.PeriodDate = (periodDisplay?.PeriodDate ?? DateTime.Now.Date);
            billPaymentVm.PeriodFrom = DateTime.Now.Date;
            billPaymentVm.PeriodTo = DateTime.Now.Date;
            modal.DataContext = billPaymentVm;
            if (ModalForm.ShowModal(modal, "Add Bill Payment", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                if ((billPaymentVm.Amount ?? 0) > 0)
                    billPaymentVm.PaymentDate = DateTime.Now.Date;
                _ = BillsPeriodContextHelper.AddUpdateBillPaymentAsync(billPaymentVm);
            }
        }

        private void BillerInfo_btn_Click(object sender, RoutedEventArgs e)
        {
            var periodVm = (sender as FrameworkElement).DataContext as BillPeriodViewModel;
            billerInfoPopup.DataContext = periodVm;
            billerInfoPopup.PlacementTarget = sender as FrameworkElement;
            billerInfoPopup.IsOpen = true;
        }

        private void billCalendar_DayClick(object sender, DayClickEventArgs e)
        {
            var btn = e.OriginalSource as MonthlyCalendarDayButton;
            PeriodCalendarDisplayCollection periodList = btn.DataContext as PeriodCalendarDisplayCollection;
            if (periodList != null)
            {
                periodListPopup.DataContext = periodList;
                periodListPopup.PlacementTarget = btn;
                periodListPopup.IsOpen = true;
            }
        }

        private void OpenVideo_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Convert.ToString((sender as FrameworkElement).Tag);
            DoOpenFile(fileName, BillsPeriodContextHelper.VIDEO_INSTRUCTIONS_FILE_SUBDIRECTORY);
        }

        private void OpenContract_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Convert.ToString((sender as FrameworkElement).Tag);
            DoOpenFile(fileName, BillsPeriodContextHelper.CONTRACTS_FILE_SUBDIRECTORY);
        }

        private void DoOpenFile(string fileName, string subdirectory)
        {
            string file = FileHelper.GetFile(fileName, subdirectory);
            if (File.Exists(file))
                FileHelper.Open(file);
            else
                MessageBox.Show("ERROR: File not found", "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Duplicate_Click(object sender, RoutedEventArgs e)
        {
            BillPeriodViewModel periodVm = (sender as FrameworkElement).DataContext as BillPeriodViewModel;
            var periodCopy = periodVm.DeepClone();
            periodCopy.Id = 0;
            periodCopy.ContractFile = string.Empty;
            var modal = new billPeriodAddEdit_modal();
            modal.DataContext = periodCopy;
            if (ModalForm.ShowModal(modal, "Add Bill Period", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _ = BillsPeriodContextHelper.AddUpdateBillPeriodAsync(periodCopy);
                billPeriodList.Insert(0, periodCopy);
                billPeriodItemList.Refresh();
                RefreshPeriodCalendarDisplay();
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

        private void PrintBillLabel_Click(object sender, RoutedEventArgs e)
        {
            string xml = WebPrintHelper.GenerateXmlForBillList(billPeriodList);
            string file = WebPrintHelper.CreatePathForPrintableXmlWithStyleSheet(xml, "billLabels");
            WebPrintHelper.Print(file, "Print Labels");
        }

        private void SelectBill_btn_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new SelectBillEventArgs(SelectBillEvent, (sender as FrameworkElement).DataContext as BillPeriodViewModel));
        }
    }

    public class SelectBillEventArgs : RoutedEventArgs
    {
        public BillPeriodViewModel Bill { get; private set; }

        public SelectBillEventArgs(RoutedEvent routedEvent, BillPeriodViewModel bill) : base(routedEvent)
        {
            this.Bill = bill;
        }
    }

    public delegate void SelectBillEventHandler(object sender, SelectBillEventArgs e);
}
