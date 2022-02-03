using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Model;

namespace Petty_Cash.Objects.CheckWriter
{
    public class CheckWriterAccountViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public CheckWriterBankViewModel Bank { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public BankAccountType AccountType { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public bool IsActive { get; set; } = true;

        public CheckWriterAccountViewModel()
        { }

        public CheckWriterAccountViewModel(bank_account entity, bank bankEntity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.Bank = new CheckWriterBankViewModel(bankEntity);
            this.AccountName = entity.AccountName;
            this.AccountNumber = entity.AccountNumber;
            this.AccountType = entity.AccountType.ToEnum<BankAccountType>();
            this.Description = entity.Description;
            this.ContactPerson = entity.ContactPerson;
            this.ContactNumber = entity.ContactNumber;
            this.Purpose = entity.Purpose;
            this.IsActive = entity.IsActive.ToBool();
        }

        public override bool Equals(object obj)
        {
            if (obj is CheckWriterAccountViewModel)
            {
                var o = obj as CheckWriterAccountViewModel;
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