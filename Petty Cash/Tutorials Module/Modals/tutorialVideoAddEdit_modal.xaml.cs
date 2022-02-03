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
using bolt5.ModalWpf;

namespace Petty_Cash.Tutorials_Module.Modals
{
    /// <summary>
    /// Interaction logic for tutorialVideoAddEdit_modal.xaml
    /// </summary>
    public partial class tutorialVideoAddEdit_modal : UserControl
    {
        public bool IsDelete { get; private set; }

        public tutorialVideoAddEdit_modal()
        {
            InitializeComponent();

            var categoryTask = TutorialsHelper.GetCategoryListAsync();
            categoryTask.Wait();
            List<TutorialCategoryViewModel> categoryList = categoryTask.Result.ToList();
            TutorialsHelper.SetCategoryHierarchy(ref categoryList);
            ICollectionView view = new CollectionViewSource() { Source = categoryList.Where(i => i.IsActive && i.IsActiveHierarchy) }.View;
            view.SortDescriptions.Add(new SortDescription("PathName", ListSortDirection.Ascending));
            categoryCbox.ItemsSource = view;
            view.Refresh();
        }

        private void Delete_btn_Click(object sender, RoutedEventArgs e)
        {
            if (ModalForm.ShowModal("DELETE this video?", "", ModalButtons.YesNo) == ModalResult.Yes)
            {
                this.IsDelete = true;
                //Close(ModalResult.Cancel);
            }
        }
    }
}
