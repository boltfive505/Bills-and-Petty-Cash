using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using Petty_Cash.Tutorials_Module;
using Petty_Cash.Tutorials_Module.Objects;
using purchase_request.Objects.Calendar;
using bolt5.CustomMonthlyCalendar;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for tutorials_calendar.xaml
    /// </summary>
    public partial class tutorials_calendar : Page
    {
        private ICollectionView periodItemList;
        private List<TutorialVideoViewModel> videoList = new List<TutorialVideoViewModel>();
        private List<PeriodCalendarDisplayCollection<TutorialVideoViewModel>> periodList = new List<PeriodCalendarDisplayCollection<TutorialVideoViewModel>>();

        public tutorials_calendar()
        {
            InitializeComponent();
            periodItemList = new CollectionViewSource() { Source = periodList }.View;
            calendar.ItemsSource = periodItemList;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var videoTask = TutorialsHelper.GetVideoList();
            videoTask.Wait();
            videoList.Clear();
            videoList.AddRange(videoTask.Result);
            //videoItemList.Refresh();

            RefreshPeriodCalendarDisplay();
        }

        private void calendar_DisplayMonthChanged(object sender, EventArgs e)
        {
            RefreshPeriodCalendarDisplay();
        }

        private void RefreshPeriodCalendarDisplay()
        {
            periodList.Clear();
            periodList.AddRange(Classes.PeriodCalendarHelper.GetPeriodListByDisplayMonth(videoList, calendar.DisplayMonth.Year, calendar.DisplayMonth.Month));
            periodItemList.Refresh();
        }

        private void calendar_DayClick(object sender, DayClickEventArgs e)
        {
            var btn = e.OriginalSource as MonthlyCalendarDayButton;
            if (btn.DataContext != null)
            {
                schedulePopup.DataContext = btn.DataContext;
                System.Diagnostics.Debug.WriteLine(schedulePopup.DataContext);
                schedulePopup.PlacementTarget = btn;
                schedulePopup.IsOpen = true;
            }
        }
    }
}
