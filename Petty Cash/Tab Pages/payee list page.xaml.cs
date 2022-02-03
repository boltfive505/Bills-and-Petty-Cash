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
using Petty_Cash.Objects;
using Petty_Cash.Classes;
using Petty_Cash.Model;
using Petty_Cash.Sub_Modals;
using bolt5.ModalWpf;
using bolt5.CloneCopy;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for payee_list_page.xaml
    /// </summary>
    public partial class payee_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
            public bool ShowInActive { get; set; }
        }

        private ICollectionView payeeItemList { get; set; }
        private List<PayeeViewModel> _payeeList = new List<PayeeViewModel>();
        private FilterGroup filters;

        public payee_list_page()
        {
            InitializeComponent();
            payeeItemList = new CollectionViewSource() { Source = _payeeList }.View;
            payeeItemList.Filter = x => DoFilterPayee(x as PayeeViewModel);
            
            payeeItemList.SortDescriptions.Add(new SortDescription("IsActive", ListSortDirection.Descending));
            payeeItemList.SortDescriptions.Add(new SortDescription("PayeeName", ListSortDirection.Ascending));
            payeeDg.ItemsSource = payeeItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _payeeList.Clear();
            _payeeList.AddRange(GetPayeeList());
            payeeItemList.Refresh();
            _ = PettyCashContextHelper.GetPayeeBalanceAsync(_payeeList);
        }

        private IEnumerable<PayeeViewModel> GetPayeeList()
        {
            var task = PettyCashContextHelper.GetPayeeListAsync(true);
            task.Wait();
            return task.Result.Where(i => i.Id != -2); //exclude special payee for editing
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            payeeItemList.Refresh();
        }

        private bool DoFilterPayee(PayeeViewModel i)
        {
            bool flag = true;

            //keyword
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
                flag &= i.PayeeName.IndexOf(filters.FilterKeyword.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0;

            //inactive
            if (!filters.ShowInActive)
                flag &= i.IsActive;
            
            return flag;
        }

        private void AddPayee_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new payeeAddEdit_modal();
            PayeeViewModel payeeVm = new PayeeViewModel();
            modal.DataContext = payeeVm;
            if (ModalForm.ShowModal(modal, "Add Payee", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _payeeList.Add(payeeVm);
                payeeItemList.Refresh();
                _ = PettyCashContextHelper.AddUpdatePayeeAsync(payeeVm);
            }
        }

        private void EditPayee_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new payeeAddEdit_modal();
            PayeeViewModel payeeVm = (sender as FrameworkElement).DataContext as PayeeViewModel;
            var clone = payeeVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Payee", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(payeeVm);
                payeeItemList.Refresh();
                _ = PettyCashContextHelper.AddUpdatePayeeAsync(payeeVm);
            }
        }
    }
}
