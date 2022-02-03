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
using Petty_Cash.Model;
using Petty_Cash.Sub_Modals;
using Petty_Cash.Classes;
using bolt5.ModalWpf;
using bolt5.CloneCopy;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for company_list_page.xaml
    /// </summary>
    public partial class company_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
        }


        private ICollectionView companyItemList;
        private List<CompanyViewModel> _companyList = new List<CompanyViewModel>();
        private FilterGroup filters;

        public company_list_page()
        {
            InitializeComponent();
            companyItemList = new CollectionViewSource() { Source = _companyList }.View;
            companyItemList.Filter = x => DoFilterCompany(x as CompanyViewModel);
            companyDg.ItemsSource = companyItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _companyList.Clear();
            _companyList.AddRange(GetCompanyList());
            companyItemList.Refresh();
        }

        private IEnumerable<CompanyViewModel> GetCompanyList()
        {
            var task = Classes.PettyCashContextHelper.GetCompanyListAsync();
            task.Wait();
            return task.Result;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            companyItemList.Refresh();
        }

        private bool DoFilterCompany(CompanyViewModel i)
        {
            bool flag = true;
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
                flag &= i.CompanyName.IndexOf(filters.FilterKeyword.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
            return flag;
        }

        private void AddCompany_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new companyAddEdit_modal();
            CompanyViewModel companyVm = new CompanyViewModel();
            modal.DataContext = companyVm;
            if (ModalForm.ShowModal(modal, "Add Company", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _companyList.Add(companyVm);
                companyItemList.Refresh();
                _ = PettyCashContextHelper.AddUpdateCompanyAsync(companyVm);
            }
        }

        private void EditCompany_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new companyAddEdit_modal();
            CompanyViewModel companyVm = (sender as FrameworkElement).DataContext as CompanyViewModel;
            CompanyViewModel clone = companyVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Company", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(companyVm);
                _ = PettyCashContextHelper.AddUpdateCompanyAsync(companyVm);
            }
        }
    }
}
