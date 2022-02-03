using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace Petty_Cash.Objects
{
    public class PettyCashViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }  //database reference
        [AlsoNotifyFor("RemainingBalance")]
        public VoucherViewModel Voucher { get; set; }
        public PayeeViewModel Payee { get; set; }
        public CategoryViewModel Category { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? AmountIn { get; set; }
        [AlsoNotifyFor("RemainingBalance", "VatAmount", "NetVatAmount", "NonVatAmount")]
        public decimal? AmountOut { get; set; }
        public decimal? AmountReturn { get; set; }
        public string Purpose { get; set; }
        public string PaymentFile { get; set; }
        public CompanyViewModel Company { get; set; }
        [AlsoNotifyFor("VatAmount", "NetVatAmount", "NonVatAmount", "Company")]
        public bool IsVat { get; set; }

        private bool _isCompanyTinRequired = true;

        public TransactionType TransactionType
        {
            get
            {
                if (AmountReturn != null)
                    return TransactionType.Return;
                if (AmountIn != null && AmountOut == null)
                    return TransactionType.Replenish;
                else if (AmountIn == null && AmountOut != null)
                    return TransactionType.Expense;
                else
                    return TransactionType.Any;
            }
        }

        public bool IsAdminExpense
        {
            get { return AmountOut != null && Payee.Id == -2; }
        }

        public decimal? VatAmount
        {
            get
            {
                if ((AmountOut == null || AmountOut.Value == 0) || !IsVat) return null;
                return Math.Round(AmountOut.Value - NetVatAmount.Value, 2);
            }
        }

        public decimal? NetVatAmount
        {
            get
            {
                if ((AmountOut == null || AmountOut.Value == 0) || !IsVat) return null;
                return Math.Round(AmountOut.Value / 1.12M, 2);
            }
        }

        public decimal? NonVatAmount
        {
            get
            {
                if ((AmountOut == null || AmountOut.Value == 0) || IsVat) return null;
                return AmountOut.Value;
            }
        }

        public decimal RemainingBalance
        {
            get
            {
                return ((Voucher?.BalanceAmount) ?? 0) - (this.AmountOut ?? 0);
            }
        }

        public string Error { get { return null; } }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == nameof(RemainingBalance))
                {
                    if (RemainingBalance < 0)
                        result = "Remaining Balance is negative. Not enough amount";
                }
                if (columnName == nameof(Company))
                {
                    if (IsVat && _isCompanyTinRequired)
                    {
                        //company/tin# is required
                        if (Company == null || string.IsNullOrEmpty(Company.TinNumber))
                            result = "VAT is applied. Must require TIN #";
                    }
                }
                return result;
            }
        }

        public void SetTinRequired(bool required)
        {
            _isCompanyTinRequired = required;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Company)));
        }

        public override bool Equals(object obj)
        {
            if (obj is PettyCashViewModel)
            {
                var o = obj as PettyCashViewModel;
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
