using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.ComponentModel;
using Petty_Cash.Tutorials_Module;
using Petty_Cash.Tutorials_Module.Objects;
using Petty_Cash.Tutorials_Module.Modals;
using Petty_Cash.Classes;
using bolt5.ModalWpf;
using bolt5.CloneCopy;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for tutorials.xaml
    /// </summary>
    public partial class tutorials : Page
    {
        public class FilterGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public string FilterDescription { get; set; }
            public int FilterSelectedCategory { get; set; }
            public bool ShowInactiveCategory { get; set; }
        }

        private ICollectionView categoryItemList;
        private ICollectionView videoItemList;
        private List<TutorialCategoryViewModel> categoryList = new List<TutorialCategoryViewModel>();
        private List<TutorialVideoViewModel> videoList = new List<TutorialVideoViewModel>();

        private FilterGroup filters;
        private FilterGroup filters2;

        public tutorials()
        {
            InitializeComponent();
            categoryItemList = new CollectionViewSource() { Source = categoryList }.View;
            categoryItemList.SortDescriptions.Add(new SortDescription("PathName", ListSortDirection.Ascending));
            categoryItemList.Filter = x => DoFilterCategory(x as TutorialCategoryViewModel);
            categoryDataGrid.ItemsSource = categoryItemList;

            videoItemList = new CollectionViewSource() { Source = videoList }.View;
            videoItemList.SortDescriptions.Add(new SortDescription("ShortDescriptionSimpleText", ListSortDirection.Ascending));
            videoItemList.Filter = x => DoFilterVideo(x as TutorialVideoViewModel);
            videoDataGrid.ItemsSource = videoItemList;

            filters = new FilterGroup();
            filters.PropertyChanged += Filters_PropertyChanged;
            filtersGroup.DataContext = filters;

            filters2 = new FilterGroup();
            filters2.PropertyChanged += Filters2_PropertyChanged;
            filtersCategoryGroup.DataContext = filters2;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var categoryTask = TutorialsHelper.GetCategoryListAsync();
            var videoTask = TutorialsHelper.GetVideoList();
            Task.WaitAll(categoryTask, videoTask);

            categoryList.Clear();
            categoryList.AddRange(categoryTask.Result);
            videoList.Clear();
            videoList.AddRange(videoTask.Result);

            TutorialsHelper.SetCategoryHierarchy(ref categoryList);
            TutorialsHelper.SetAllCategoryListVideoCounting(ref categoryList, videoList);
            categoryItemList.Refresh();
            videoItemList.Refresh();

            ExpandAllCategoryList(true);
        }

        private void CategoryRow_Selected(object sender, RoutedEventArgs e)
        {
            TutorialCategoryViewModel cat = (sender as FrameworkElement).DataContext as TutorialCategoryViewModel;
            filters.FilterSelectedCategory = cat == null ? 0 : cat.Id;
        }

        private void VideoRow_Selected(object sender, RoutedEventArgs e)
        {
            //TutorialVideoViewModel video = (sender as FrameworkElement).DataContext as TutorialVideoViewModel;
            //videoDetailsGroup.DataContext = video;
        }

        private bool DoFilterCategory(TutorialCategoryViewModel i)
        {
            bool flag = true;
            if (!filters2.ShowInactiveCategory)
                flag &= i.IsActive && i.IsActiveHierarchy;
            return flag;
        }

        private bool DoFilterVideo(TutorialVideoViewModel i)
        {
            bool flag = true;

            //selected category
            flag &= i.Category.Id == filters.FilterSelectedCategory;

            //short and long description
            if (!string.IsNullOrWhiteSpace(filters.FilterDescription))
            {
                string keyword = filters.FilterDescription.Trim();
                string description = i.ShortDescriptionSimpleText + " " + i.LongDescriptionSimpleText;
                flag &= description.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) != -1;
            }

            return flag;
        }

        private void Filters_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            videoItemList.Refresh();
        }

        private void Filters2_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            categoryItemList.Refresh();
        }

        private void OpenVideo_btn_Click(object sender, RoutedEventArgs e)
        {
            TutorialVideoViewModel video = (sender as FrameworkElement).DataContext as TutorialVideoViewModel;
            string file = FileHelper.GetFile(video.VideoFile, TutorialsHelper.TUTORIAL_VIDEO_FOLDER);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                MessageBox.Show("ERROR: File not found.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRawVideo_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Convert.ToString((sender as FrameworkElement).Tag);
            string file = FileHelper.GetFile(fileName, TutorialsHelper.TUTORIAL_RAW_VIDEO_FOLDER);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                MessageBox.Show("ERROR: File not found.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenFileAttachment_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Convert.ToString((sender as FrameworkElement).Tag);
            string file = FileHelper.GetFile(fileName, TutorialsHelper.TUTORIAL_FILES_FOLDER);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                MessageBox.Show("ERROR: File not found.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenYoutubeLink_btn_Click(object sender, RoutedEventArgs e)
        {
            string link = Convert.ToString((sender as FrameworkElement).Tag);
            System.Diagnostics.Process.Start(link);
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var modal = new tutorialCategoryAddEdit_modal();
            var categoryVm = new TutorialCategoryViewModel();
            modal.DataContext = categoryVm;
            if (ModalForm.ShowModal(modal, "Add Category", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                var t = TutorialsHelper.AddUpdateCategory(categoryVm);
                t.Wait();
                categoryList.Add(categoryVm);
                DoPostAddUpdateCategory(categoryVm);
            }
        }

        private void EditCategory_btn_Click(object sender, RoutedEventArgs e)
        {
            var modal = new tutorialCategoryAddEdit_modal();
            var categoryVm = (sender as FrameworkElement).DataContext as TutorialCategoryViewModel;
            var clone = categoryVm.DeepClone();
            modal.DataContext = clone;
            if (ModalForm.ShowModal(modal, "Edit Category", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                clone.DeepCopyTo(categoryVm);
                var t = TutorialsHelper.AddUpdateCategory(categoryVm);
                t.Wait();
                DoPostAddUpdateCategory(categoryVm);
            }
        }

        private void AddVideo_Click(object sender, RoutedEventArgs e)
        {
            var modal = new tutorialVideoAddEdit_modal();
            var videoVm = new TutorialVideoViewModel();
            //check if click is from category
            var cat = (sender as FrameworkElement).DataContext as TutorialCategoryViewModel;
            if (cat != null)
                videoVm.Category = cat;
            modal.DataContext = videoVm;
            if (ModalForm.ShowModal(modal, "Add Video", ModalButtons.SaveCancel) == ModalResult.Save)
            {
                _ = TutorialsHelper.AddUpdateVideo(videoVm);
                videoList.Add(videoVm);
                videoItemList.Refresh();
                TutorialsHelper.SetAllCategoryListVideoCounting(ref categoryList, videoList);
            }
        }

        private void EditVideo_Click(object sender, RoutedEventArgs e)
        {
            var modal = new tutorialVideoAddEdit_modal();
            var videoVm = (sender as FrameworkElement).DataContext as TutorialVideoViewModel;
            var clone = videoVm.DeepClone();
            modal.DataContext = clone;
            var result = ModalForm.ShowModal(modal, "Edit Video", ModalButtons.SaveCancel);
            if (result == ModalResult.Save)
            {
                clone.DeepCopyTo(videoVm);
                _ = TutorialsHelper.AddUpdateVideo(videoVm);
                videoItemList.Refresh();
                TutorialsHelper.SetAllCategoryListVideoCounting(ref categoryList, videoList);
            }
            else if (result == ModalResult.Cancel && modal.IsDelete)
            {
                //delete video
                _ = TutorialsHelper.DeleteVideo(videoVm);
                videoList.Remove(videoVm);
                videoItemList.Refresh();
                videoDataGrid.UnselectAll();
                TutorialsHelper.SetAllCategoryListVideoCounting(ref categoryList, videoList);
            }
        }

        private void DoPostAddUpdateCategory(TutorialCategoryViewModel categoryVm)
        {
            TutorialsHelper.SetCategoryHierarchy(ref categoryList);
            if (categoryVm.ParentCategory != null)
                categoryVm.ParentCategory.IsCategoryExpanded = true;
            categoryItemList.Refresh();
            categoryDataGrid.SelectedValue = categoryVm.Id;
        }

        private void ExpandAll_btn_Click(object sender, RoutedEventArgs e)
        {
            ExpandAllCategoryList(true);
        }

        private void CollapseAll_btn_Click(object sender, RoutedEventArgs e)
        {
            ExpandAllCategoryList(false);
        }

        private void ExpandAllCategoryList(bool expand)
        {
            foreach (var cat in categoryList)
                cat.IsCategoryExpanded = expand;
        }
    }
}
