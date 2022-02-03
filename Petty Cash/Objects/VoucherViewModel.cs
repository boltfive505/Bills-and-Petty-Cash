using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Model;
using Petty_Cash.Objects.CheckWriter;

namespace Petty_Cash.Objects
{
    public class VoucherViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; } //database reference
        public PayeeViewModel Payee { get; set; }
        public DateTime VoucherDate { get; set; }
        public decimal? AmountReceived { get; set; }
        public decimal? AmountReplenished { get; set; }
        public bool IsLiquidated { get; set; }
        public string Particulars { get; set; }
        public CheckWriterCheckViewModel Check { get; set; }

        public decimal ExpenseAmount { get; set; }
        public decimal ReturnAmount { get; set; }

        public decimal BalanceAmount 
        { 
            get { return (AmountReceived ?? 0) - (ExpenseAmount + ReturnAmount); }
        }

        public string VoucherNumber
        {
            get { return this.Id.ToString(); }
        }

        public bool HasAmountReplenished
        {
            get { return AmountReplenished != 0; }
        }

        public bool HasExpenseAmount
        {
            get { return ExpenseAmount != 0; }
        }

        public int OpenStatus
        {
            get
            {
                if (!IsLiquidated)
                {
                    if (ExpenseAmount == 0)
                        return 1;
                    else
                        return 2;
                }
                return 0;
            }
        }

        public TransactionType TransactionType
        {
            get
            {
                if (AmountReplenished != null)
                    return TransactionType.Replenish;
                else if (AmountReceived != null)
                    return TransactionType.Expense;
                else
                    return TransactionType.Any;
            }
        }

        public VoucherViewModel()
        { }

        public VoucherViewModel(voucher entity, payee payeeEntity)
        {
            if (entity == null) return;
            this.Id = (int)entity.Id;
            this.Payee = new PayeeViewModel(payeeEntity);
            this.VoucherDate = entity.VoucherDate.ToUnixDate();
            this.AmountReceived = entity.AmountReceived;
            this.AmountReplenished = entity.AmountReplenished;
            this.IsLiquidated = entity.IsLiquidated.ToBool();
            this.Particulars = entity.Particulars;
        }

        public VoucherViewModel(voucher entity, payee payeeEntity, bank_check checkEntity) : this(entity, payeeEntity)
        {
            this.Check = checkEntity == null ? null : new CheckWriterCheckViewModel(checkEntity, null, null, null);
        }

        public override bool Equals(object obj)
        {
            if (obj is VoucherViewModel)
            {
                var o = obj as VoucherViewModel;
                return o.Id == this.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
