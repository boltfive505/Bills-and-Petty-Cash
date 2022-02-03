using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Model;

namespace Petty_Cash.Objects.CheckWriter
{
    public class CheckWriterBankViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public CheckWriterBankViewModel()
        { }

        public CheckWriterBankViewModel(bank entity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.BankName = entity.BankName;
            this.Branch = entity.Branch;
            this.Description = entity.Description;
            this.IsActive = entity.IsActive.ToBool();
        }

        public override bool Equals(object obj)
        {
            if (obj is CheckWriterBankViewModel)
            {
                var o = obj as CheckWriterBankViewModel;
                return this.Id == o.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
