using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Petty_Cash.Classes;
using Petty_Cash.Objects;

namespace Petty_Cash.Sub_Modals.CheckWriter
{
    /// <summary>
    /// Interaction logic for check_add_modal.xaml
    /// </summary>
    public partial class check_add_modal : UserControl
    {
        public class SelectPayeeGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public ObservableCollection<PayeeViewModel> PayeeList { get; set; }
            public string SelectedPayeeName { get; set; }
        }

        private SelectPayeeGroup selectPayee;

        public check_add_modal()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var list = BillsPeriodContextHelper.GetBillPeriodSimpleListAsync().GetResult()
                                        .Where(i => i.IsActive && i.Category.IsActive)
                                        .Select(i => string.Format("{0} - {1}", i.BillerName, i.BillDescription));
            ObservableCollection<string> payToBillList = new ObservableCollection<string>(list);
            payToBillList.Insert(0, "");
            PayToBillValue.ItemsSource = payToBillList;
            //payee list
            selectPayee = new SelectPayeeGroup();
            selectPayee.PayeeList = new ObservableCollection<PayeeViewModel>(PettyCashContextHelper.GetPayeeListAsync(false).GetResult());
            selectPayee.PropertyChanged += SelectPayee_PropertyChanged;
            selectPayeePopup.DataContext = selectPayee;
        }

        private void SelectPayee_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(selectPayee.SelectedPayeeName))
            {
                if (!string.IsNullOrEmpty(selectPayee.SelectedPayeeName))
                {
                    PayeeValue.Text = selectPayee.SelectedPayeeName;
                    selectPayee.SelectedPayeeName = null;
                    selectPayeePopup.IsOpen = false;
                }
            }
        }

        private void SelectPayee_Click(object sender, RoutedEventArgs e)
        {
            selectPayeePopup.PlacementTarget = sender as UIElement;
            selectPayeePopup.IsOpen = true;
        }
    }
}
