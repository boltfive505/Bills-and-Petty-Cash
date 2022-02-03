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
    /// Interaction logic for category_list_page.xaml
    /// </summary>
    public partial class department_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
        }


        private ICollectionView departmentItemList;
        private List<DepartmentViewModel> _departmentList = new List<DepartmentViewModel>();
        private FilterGroup filters;

        public department_list_page()
        {
            InitializeComponent();
            departmentItemList = new CollectionViewSource() { Source = _departmentList }.View;
            departmentItemList.Filter = x => DoFilterDepartment(x as DepartmentViewModel);
            departmentDg.ItemsSource = departmentItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _departmentList.Clear();
            _departmentList.AddRange(GetDepartmentList());
            departmentItemList.Refresh();
        }

        private IEnumerable<DepartmentViewModel> GetDepartmentList()
        {
            var task = Classes.PettyCashContextHelper.GetDepartmentListAsync();
            task.Wait();
            return task.Result;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            departmentItemList.Refresh();
        }

        private bool DoFilterDepartment(DepartmentViewModel i)
        {
            bool flag = true;
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
                flag &= i.DepartmentName.IndexOf(filters.FilterKeyword.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
            return flag;
        }

        private void AddDepartment_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new departmentAddEdit_modal();
            DepartmentViewModel departmentVm = new DepartmentViewModel();
            modal.DataContext = departmentVm;
            if (ModalForm.ShowModal(modal, "Add Department", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _departmentList.Add(departmentVm);
                departmentItemList.Refresh();
                _ = PettyCashContextHelper.AddUpdateDepartmentAsync(departmentVm);
            }
        }

        private void EditDepartment_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new departmentAddEdit_modal();
            DepartmentViewModel departmentVm = (sender as FrameworkElement).DataContext as DepartmentViewModel;
            var clone = departmentVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Department", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(departmentVm);
                _ = PettyCashContextHelper.AddUpdateDepartmentAsync(departmentVm);
            }
        }
    }
}
