using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Petty_Cash.Converters;

namespace Petty_Cash.Objects.Bills
{
    public class BillPeriodViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        //bill contract info
        public string BillerName { get; set; }
        public CategoryViewModel Category { get; set; }
        public string Description { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string UnitNumber { get; set; }
        public string PhoneNumber { get; set; }
        public decimal? ContractAmount { get; set; }
        public DateTime? ContractExpiration { get; set; }
        public string ContractFile { get; set; }
        public string ReferenceLink { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string BillDescription { get; set; }
        public string BillInstructions { get; set; }
        public string VideoInstructionsFile { get; set; }
        public bool IsVat { get; set; }
        public BillTaxType TaxType { get; set; } = BillTaxType.Not_Applicable;
        public decimal? TaxRate { get; set; }
        public string TinNumber { get; set; }

        //bill period info
        public BillPeriodType PeriodType { get; set; } = BillPeriodType.Monthly;
        public DateTime? DueDateStart { get; set; }
        public DateTime? DueDateEnd { get; set; }
        public int? DueMonth { get; set; }
        public int? DueDays { get; set; }
        public bool IsActive { get; set; } = true;

        public List<BillPeriodInclusionViewModel> MonthInclusionList { get; set; }
        public List<BillPeriodInclusionViewModel> QuarterInclusionList { get; set; }

        public List<DateTime> MissingBillsByDueDate { get; set; } = new List<DateTime>();
        public List<DateTime> MissingBillsAmountFile { get; set; } = new List<DateTime>();
        public List<DateTime> MissingReceiptAmountFile { get; set; } = new List<DateTime>();

        public int NumberOfPendingPayment { get; set; }

        private static OrdinalStringConverter _ordinalConverter;
        private static MonthNameConverter _monthNameConverter;

        public string TaxTypeRateDisplay
        {
            get
            {
                switch (TaxType)
                {
                    case BillTaxType.Vat:
                        return string.Format("VAT {0}%", TaxRate);
                    case BillTaxType.Withholding_Tax:
                        return string.Format("WHT {0}%", TaxRate);
                    default:
                        return string.Empty;
                }
            }
        }

        public string DueDateStr
        {
            get
            {
                switch (PeriodType)
                {
                    case BillPeriodType.OneTime:
                        return DueDateStart.Value.ToString("MMM dd, yyyy");
                    case BillPeriodType.Monthly:
                    case BillPeriodType.EndOfQuarter:
                        return string.Format("{0} day", _ordinalConverter.Convert(DueDays, null, null, null));
                    case BillPeriodType.Annually:
                        return string.Format("{0} {1}", _monthNameConverter.Convert(DueMonth, null, null, null), DueDays);
                    default:
                        return "";
                }
            }
        }

        public bool HasExclusion
        {
            get
            {
                switch (PeriodType)
                {
                    case BillPeriodType.Monthly:
                        return MonthInclusionList.Any(i => !i.IsIncluded);
                    case BillPeriodType.EndOfQuarter:
                        return QuarterInclusionList.Any(i => !i.IsIncluded);
                    default:
                        return false;
                }
            }
        }

        public string Error { get { return null; } }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                if (columnName == "Category")
                {
                    if (this.Category == null)
                        result = "Category is required";
                }
                return result;
            }
        }

        static BillPeriodViewModel()
        {
            _ordinalConverter = new OrdinalStringConverter();
            _monthNameConverter = new MonthNameConverter();
        }

        public BillPeriodViewModel()
        {
            MonthInclusionList = Enumerable.Range(1, 12)
                .Select(n => new BillPeriodInclusionViewModel() { Value = n })
                .ToList();

            QuarterInclusionList = Enumerable.Range(1, 4)
                .Select(n => new BillPeriodInclusionViewModel() { Value = n })
                .ToList();
        }

        public IEnumerable<DateTime> GetPeriodDatesByYear(int year)
        {
            List<DateTime> dates = new List<DateTime>();
            try
            {
                switch (PeriodType)
                {
                    case BillPeriodType.OneTime:
                        dates.Add(DueDateStart.Value);
                        break;
                    case BillPeriodType.Monthly:
                        dates.AddRange(GetMonthlyDates(year, DueDays.Value));
                        break;
                    case BillPeriodType.EndOfQuarter:
                        dates.AddRange(GetEndOfQuarterDates(year, DueDays.Value));
                        break;
                    case BillPeriodType.Annually:
                        dates.Add(new DateTime(year, DueMonth.Value, DueDays.Value));
                        break;
                }
            }
            catch
            { }
            return dates;
        }

        private IEnumerable<DateTime> GetMonthlyDates(int year, int day)
        {
            return Enumerable.Range(1, 12)
                .Where(n => MonthInclusionList.First(i => i.Value == n).IsIncluded)
                .Select(n => 
                {
                    int daysInMonth = DateTime.DaysInMonth(year, n);
                    return new DateTime(year, n, Math.Min(day, daysInMonth));
                });
        }

        private IEnumerable<DateTime> GetEndOfQuarterDates(int year, int daysOffset)
        {
            List<DateTime> dates = new List<DateTime>();
            //include last quarter from last year
            if (QuarterInclusionList.First(i => i.Value == 4).IsIncluded)
            {
                dates.Add(new DateTime(year - 1, 12, DateTime.DaysInMonth(year - 1, 12)).AddDays(daysOffset));
            }

            //add from first 3 quarters of year
            //not include last quarter of this year
            dates.AddRange(Enumerable.Range(1, 3)
                .Where(n => QuarterInclusionList.First(i => i.Value == n).IsIncluded)
                .Select(n =>
                {
                    int month = n * 3;
                    return new DateTime(year, month, DateTime.DaysInMonth(year, month)).AddDays(daysOffset);
                }));
            return dates;
        }

        public override bool Equals(object obj)
        {
            if (obj is BillPeriodViewModel)
            {
                var o = obj as BillPeriodViewModel;
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
