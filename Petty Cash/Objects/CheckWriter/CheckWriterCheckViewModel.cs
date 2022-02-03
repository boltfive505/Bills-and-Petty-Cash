using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Model;

namespace Petty_Cash.Objects.CheckWriter
{
    public class CheckWriterCheckViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public CheckWriterAccountViewModel Account { get; set; }
        public VoucherViewModel Voucher { get; set; }
        public string CheckNumber { get; set; }
        public decimal Amount { get; set; }
        public string PayeeName { get; set; }
        public DateTime CheckDate { get; set; } = DateTime.Now;
        public string Description { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public DateTime? PrintedDate { get; set; }
        public string PayToBillName { get; set; }
        public bool AlreadyUsed { get; set; }

        public CheckWriterCheckViewModel()
        { }

        public CheckWriterCheckViewModel(bank_check entity, voucher voucherEntity, bank_account accountEntity, bank bankEntity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.Account = new CheckWriterAccountViewModel(accountEntity, bankEntity);
            this.Voucher = voucherEntity == null ? null : new VoucherViewModel(voucherEntity, null);
            this.CheckNumber = entity.CheckNumber;
            this.Amount = entity.Amount;
            this.PayeeName = entity.PayeeName;
            this.CheckDate = entity.CheckDate.ToUnixDate();
            this.Description = entity.Description;
            this.UpdatedDate = entity.UpdatedDate.ToUnixDate();
            this.PrintedDate = entity.PrintedDate.ToUnixDate();
            this.PayToBillName = entity.PayToBillName;
            this.AlreadyUsed = entity.AlreadyUsed.ToBool();
        }
    }
}
