using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Objects
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; } //database reference
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public CategoryViewModel()
        { }

        public CategoryViewModel(Petty_Cash.Model.category entity)
        {
            if (entity == null) return;
            Id = (int)entity.Id;
            CategoryName = entity.CategoryName;
            Description = entity.Description;
        }

        public CategoryViewModel(Petty_Cash.Bills.Model.category entity)
        {
            if (entity == null) return;
            Id = (int)entity.Id;
            CategoryName = entity.CategoryName;
            Description = entity.Description;
            IsActive = entity.IsActive.ToBool();
        }

        public override bool Equals(object obj)
        {
            if (obj is CategoryViewModel)
            {
                CategoryViewModel o = obj as CategoryViewModel;
                return this.Id == o.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
