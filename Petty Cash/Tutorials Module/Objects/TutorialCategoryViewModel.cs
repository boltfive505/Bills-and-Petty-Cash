using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorials.Model;
using bolt5.CloneCopy;

namespace Petty_Cash.Tutorials_Module.Objects
{
    public class TutorialCategoryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; } //database reference
        public int? ParentCategoryId { get; set; } //database reference
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        private TutorialCategoryViewModel _parentCategory;
        [CloneCopyIgnore]
        public TutorialCategoryViewModel ParentCategory
        {
            get { return _parentCategory; }
            set
            {
                //remove event
                if (_parentCategory != null)
                    _parentCategory.PropertyChanged -= _parentCategory_PropertyChanged;
                _parentCategory = value;
                //add event
                if (_parentCategory != null)
                    _parentCategory.PropertyChanged += _parentCategory_PropertyChanged;
                OnCategoryParentChanged();
            }
        }

        [CloneCopyIgnore]
        public int HierarchyLevel { get; set; }
        [CloneCopyIgnore]
        public string PathName { get; set; }
        [CloneCopyIgnore]
        public bool IsCategoryExpanded { get; set; }
        [CloneCopyIgnore]
        public bool HasCategoryChildren { get; set; }
        [CloneCopyIgnore]
        public bool IsCategoryVisible { get; set; } = true;
        [CloneCopyIgnore]
        public int? VideoCount { get; set; }
        [CloneCopyIgnore]
        public bool IsActiveHierarchy { get; set; }
        [CloneCopyIgnore] 
        public bool IsSelectable { get; set; } //only used for select parent category -> to prevent self parent stackoverflow

        public TutorialCategoryViewModel()
        {
        }

        public TutorialCategoryViewModel(category entity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.CategoryName = entity.CategoryName;
            this.Description = entity.Description;
            this.IsActive = entity.IsActive.ToBool();
            this.ParentCategoryId = (int?)entity.ParentCategoryId;
        }

        private void _parentCategory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsCategoryExpanded) || e.PropertyName == nameof(IsCategoryVisible))
            {
                OnCategoryParentChanged();
            }
        }

        private void OnCategoryParentChanged()
        {
            this.IsCategoryVisible = this.ParentCategory == null || (this.ParentCategory != null && this.ParentCategory.IsCategoryExpanded && this.ParentCategory.IsCategoryVisible);
        }

        public override bool Equals(object obj)
        {
            if (obj is TutorialCategoryViewModel)
            {
                TutorialCategoryViewModel o = obj as TutorialCategoryViewModel;
                return o.Id == this.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
