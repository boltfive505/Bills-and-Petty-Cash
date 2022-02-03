using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;
using Petty_Cash.Objects.Bills;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for billPeriodAddEdit_modal.xaml
    /// </summary>
    public partial class billPeriodAddEdit_modal : UserControl, IModalClosing, IModalClosed
    {
        public class BillTaxTypePair : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public BillTaxType TaxType { get; set; }
            public decimal? TaxRate { get; set; }
        }


        ObservableCollection<BillTaxTypePair> taxList;

        public billPeriodAddEdit_modal()
        {
            InitializeComponent();
            //load categories
            var categoryTask = Classes.BillsPeriodContextHelper.GetCategoryListAsync();
            categoryTask.Wait();
            CategoryValue.ItemsSource = new ObservableCollection<Objects.CategoryViewModel>(categoryTask.Result);

            taxList = new ObservableCollection<BillTaxTypePair>();
            taxListBox.ItemsSource = taxList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            taxList.Add(new BillTaxTypePair() { TaxType = BillTaxType.Not_Applicable });
            taxList.Add(new BillTaxTypePair() { TaxType = BillTaxType.Vat, TaxRate = 12 });
            BillTaxTypePair wht = new BillTaxTypePair() { TaxType = BillTaxType.Withholding_Tax };
            //manually update tax rate if WHT
            //The error -> not updating properly
            BillPeriodViewModel periodVm = this.DataContext as BillPeriodViewModel;
            if (periodVm.TaxType == BillTaxType.Withholding_Tax)
                wht.TaxRate = periodVm.TaxRate;
            taxList.Add(wht);
        }

        public void ModalClosing(ModalClosingArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                if (CategoryValue.GetBindingExpression(ComboBox.SelectedItemProperty).HasError)
                    e.Cancel = true;
            }
        }

        public void ModalClosed(ModalClosedArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                //manually update back if selection is WHT
                var tax = taxList.FirstOrDefault(i => i.TaxType == (taxListBox.SelectedItem as BillTaxTypePair).TaxType);
                BillPeriodViewModel periodVm = this.DataContext as BillPeriodViewModel;
                periodVm.TaxRate = tax.TaxRate;
            }
        }
    }
}
