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
    public partial class category_list_page : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterKeyword { get; set; }
        }


        private ICollectionView categoryItemList;
        private List<CategoryViewModel> _categoryList = new List<CategoryViewModel>();
        private FilterGroup filters;

        public category_list_page()
        {
            InitializeComponent();
            categoryItemList = new CollectionViewSource() { Source = _categoryList }.View;
            categoryItemList.Filter = x => DoFilterPayee(x as CategoryViewModel);
            categoryDg.ItemsSource = categoryItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _categoryList.Clear();
            _categoryList.AddRange(GetCategoryList());
            categoryItemList.Refresh();
        }

        private IEnumerable<CategoryViewModel> GetCategoryList()
        {
            var task = Classes.PettyCashContextHelper.GetCategoryListAsync();
            task.Wait();
            return task.Result;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            categoryItemList.Refresh();
        }

        private bool DoFilterPayee(CategoryViewModel i)
        {
            bool flag = true;
            if (!string.IsNullOrWhiteSpace(filters.FilterKeyword))
                flag &= i.CategoryName.IndexOf(filters.FilterKeyword.Trim(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
            return flag;
        }

        private void AddCategory_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new categoryAddEdit_modal();
            CategoryViewModel categoryVm = new CategoryViewModel();
            modal.DataContext = categoryVm;
            if (ModalForm.ShowModal(modal, "Add Category", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _categoryList.Add(categoryVm);
                categoryItemList.Refresh();
                _ = PettyCashContextHelper.AddUpdateCategoryAsync(categoryVm);
            }
        }

        private void EditCategory_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new categoryAddEdit_modal();
            CategoryViewModel categoryVm = (sender as FrameworkElement).DataContext as CategoryViewModel;
            var clone = categoryVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Category", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(categoryVm);
                _ = PettyCashContextHelper.AddUpdateCategoryAsync(categoryVm);
            }
        }
    }
}
