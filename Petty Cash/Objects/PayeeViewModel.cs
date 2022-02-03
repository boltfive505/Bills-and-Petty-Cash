using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petty_Cash.Objects
{
    public class PayeeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; } //database reference
        public string PayeeName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public decimal Amount { get; set; } //only use for amount display

        private static PayeeViewModel _adminPayee; //special payee
        public static PayeeViewModel AdminPayee
        {
            get
            {
                if (_adminPayee == null)
                {
                    _adminPayee = new PayeeViewModel()
                    {
                        Id = -2,
                        PayeeName = "Admin Expense",
                        Description = "This is a special payee account",
                        IsActive = true
                    };
                }
                return _adminPayee;
            }
        }

        public PayeeViewModel()
        { }

        public PayeeViewModel(Petty_Cash.Model.payee entity)
        {
            if (entity == null) return;
            Id = (int)entity.Id;
            PayeeName = entity.PayeeName;
            Description = entity.Description;
            IsActive = entity.IsActive.ToBool();
        }

        public override bool Equals(object obj)
        {
            if (obj is PayeeViewModel)
            {
                PayeeViewModel o = obj as PayeeViewModel;
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
