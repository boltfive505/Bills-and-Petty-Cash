using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Objects
{
    public class CompanyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string TinNumber { get; set; }

        public bool IsVatable { get { return !string.IsNullOrEmpty(TinNumber); } }

        public CompanyViewModel()
        { }

        public CompanyViewModel(Petty_Cash.Model.company entity)
        {
            if (entity == null) return;
            Id = (int)entity.Id;
            CompanyName = entity.CompanyName;
            Address = entity.Address;
            TinNumber = entity.TinNumber.Replace("-","");
        }

        public override bool Equals(object obj)
        {
            if (obj is CompanyViewModel)
            {
                CompanyViewModel o = obj as CompanyViewModel;
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
