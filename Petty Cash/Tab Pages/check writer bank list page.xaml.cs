using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using Petty_Cash.Objects.CheckWriter;
using Petty_Cash.Sub_Modals.CheckWriter;
using Petty_Cash.Classes;
using bolt5.ModalWpf;
using bolt5.CloneCopy;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for category_list_page.xaml
    /// </summary>
    public partial class check_writer_bank_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
            public bool ShowInActive { get; set; }
        }

        private ICollectionView bankView;
        private List<CheckWriterBankViewModel> bankList = new List<CheckWriterBankViewModel>();
        private FilterGroup filters;

        public check_writer_bank_list_page()
        {
            InitializeComponent();
            bankView = new CollectionViewSource() { Source = bankList }.View;
            bankView.SortDescriptions.Add(new SortDescription("BankName", ListSortDirection.Ascending));
            bankView.Filter = x => DoFilterBank(x as CheckWriterBankViewModel);
            bankDatagrid.ItemsSource = bankView;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            bankList.Clear();
            bankList.AddRange(CheckWriterHelper.GetBankListAsync().GetResult());
            bankView.Refresh();
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bankView.Refresh();
        }

        private bool DoFilterBank(CheckWriterBankViewModel i)
        {
            bool flag = true;
            //keyword
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
            {
                string keyword = filters.FilterKeyword.Trim();
                flag &= i.BankName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                        i.Branch.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            //show inactive
            if (!filters.ShowInActive)
                flag &= i.IsActive;
            return flag;
        }

        private void AddBank_Click(object sender, RoutedEventArgs e)
        {
            bool isEdit = true;
            string title = "Edit Bank";
            var bank = (sender as FrameworkElement).DataContext as CheckWriterBankViewModel;
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
                    bankList.Add(bank);
                bankView.Refresh();
            }
        }
    }
}
