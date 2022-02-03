using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.IO;
using Petty_Cash.Objects;
using Petty_Cash.Objects.Bills;
using Petty_Cash.Bills.Model;

namespace Petty_Cash.Classes
{
    public static class BillsPeriodContextHelper
    {
        public const string BILLS_FILE_SUBDIRECTORY = "uploads\\bills\\payment";
        public const string CONTRACTS_FILE_SUBDIRECTORY = "uploads\\bills\\contract";
        public const string VIDEO_INSTRUCTIONS_FILE_SUBDIRECTORY = "uploads\\bills\\video instructions";

        public static async Task AddUpdateBillPeriodAsync(BillPeriodViewModel periodVm)
        {
            using (var context = new BillsModel())
            {
                var billPeriod = await context.bill_period.FirstOrDefaultAsync(i => i.Id == periodVm.Id);
                if (billPeriod == null)
                {
                    billPeriod = new bill_period();
                    context.bill_period.Add(billPeriod);
                }
                //bill contract info
                billPeriod.BillerName = periodVm.BillerName;
                billPeriod.CategoryId = periodVm.Category.Id;
                billPeriod.Description = periodVm.Description;
                billPeriod.AccountName = periodVm.AccountName;
                billPeriod.AccountNumber = periodVm.AccountNumber;
                billPeriod.ContactPerson = periodVm.ContactPerson;
                billPeriod.ContactNumber = periodVm.ContactNumber;
                billPeriod.Address = periodVm.Address;
                billPeriod.UnitNumber = periodVm.UnitNumber;
                billPeriod.PhoneNumber = periodVm.PhoneNumber;
                billPeriod.ContractAmount = periodVm.ContractAmount;
                billPeriod.ContractExpiration = periodVm.ContractExpiration.ToUnixLong();
                billPeriod.ReferenceLink = periodVm.ReferenceLink;
                billPeriod.Username = periodVm.Username;
                billPeriod.Password = periodVm.Password;
                billPeriod.BillDescription = periodVm.BillDescription;
                billPeriod.BillInstructions = periodVm.BillInstructions;
                billPeriod.IsVat = (int)periodVm.IsVat.ToLong();
                billPeriod.TaxType = periodVm.TaxType.ToString();
                billPeriod.TaxRate = periodVm.TaxRate;
                billPeriod.TinNumber = periodVm.TinNumber;
                //file attachments
                billPeriod.ContractFile = ProcessFile(billPeriod.ContractFile, periodVm.ContractFile, BillsPeriodContextHelper.CONTRACTS_FILE_SUBDIRECTORY, string.Format("contract_{0}_{1}", periodVm.BillerName, periodVm.Category.CategoryName));
                billPeriod.VideoInstructionsFile = ProcessFile(billPeriod.VideoInstructionsFile, periodVm.VideoInstructionsFile, BillsPeriodContextHelper.VIDEO_INSTRUCTIONS_FILE_SUBDIRECTORY, string.Format("video instructions_{0}_{1}", periodVm.BillerName, periodVm.Category.CategoryName));
                //bill period info
                billPeriod.PeriodType = periodVm.PeriodType.ToString();
                billPeriod.DueDateStart = periodVm.DueDateStart.ToUnixLong();
                billPeriod.DueDateEnd = periodVm.DueDateEnd.ToUnixLong();
                billPeriod.DueMonth = periodVm.DueMonth;
                billPeriod.DueDays = periodVm.DueDays;
                billPeriod.IsActive = periodVm.IsActive.ToLong();
                //inclusion
                var inclusionList = await (from incl in context.period_inclusion
                                           where incl.PeriodId == periodVm.Id
                                           select incl).ToListAsync();
                //inclusion month
                foreach (var inclVm in periodVm.MonthInclusionList)
                {
                    period_inclusion incl = inclusionList.FirstOrDefault(i => i.Id == inclVm.Id);
                    if (incl == null)
                    {
                        incl = new period_inclusion();
                        incl.bill_period = billPeriod;
                        context.period_inclusion.Add(incl);
                    }
                    incl.InclusionType = BillPeriodType.Monthly.ToString();
                    incl.Value = inclVm.Value;
                    incl.IsIncluded = inclVm.IsIncluded.ToLong();
                }
                //inclusion quarter
                foreach (var inclVm in periodVm.QuarterInclusionList)
                {
                    period_inclusion incl = inclusionList.FirstOrDefault(i => i.Id == inclVm.Id);
                    if (incl == null)
                    {
                        incl = new period_inclusion();
                        incl.bill_period = billPeriod;
                        context.period_inclusion.Add(incl);
                    }
                    incl.InclusionType = BillPeriodType.EndOfQuarter.ToString();
                    incl.Value = inclVm.Value;
                    incl.IsIncluded = inclVm.IsIncluded.ToLong();
                }

                await context.SaveChangesAsync();

                periodVm.Id = (int)billPeriod.Id;
            }
        }

        public static async Task AddUpdateBillPaymentAsync(BillPaymentViewModel paymentVm)
        {
            using (var context = new BillsModel())
            {
                var payment = await context.bill_payment.FirstOrDefaultAsync(i => i.Id == paymentVm.Id);
                if (payment == null)
                {
                    payment = new bill_payment();
                    context.bill_payment.Add(payment);
                }
                //payment
                payment.PeriodId = paymentVm.Period.Id;
                payment.PeriodDate = paymentVm.PeriodDate.ToUnixLong();
                payment.PaymentDate = paymentVm.PaymentDate.ToUnixLong();
                payment.Amount = paymentVm.Amount;
                payment.Notes = paymentVm.Notes;
                payment.PeriodFrom = paymentVm.PeriodFrom.ToUnixLong();
                payment.PeriodTo = paymentVm.PeriodTo.ToUnixLong();
                payment.BillAmount = paymentVm.BillAmount;
                payment.BillReferenceNumber = paymentVm.BillReferenceNumber;
                payment.PaymentType = paymentVm.PaymentType;
                payment.VatAmount = paymentVm.VatAmount;
                payment.NetVatAmount = paymentVm.NetVatAmount;
                //attachment files
                payment.BillFile = ProcessFile(payment.BillFile, paymentVm.BillFileName, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY, string.Format("bill_{0}", paymentVm.Period?.BillerName));
                payment.ReceiptFile = ProcessFile(payment.ReceiptFile, paymentVm.ReceiptFileName, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY, string.Format("receipt_{0}", paymentVm.Period?.BillerName));

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.ToString());
                }
                paymentVm.Id = (int)payment.Id;
            }
        }

        private static void DeleteFile(string oldFileName, string subdirectory)
        {
            //delete old file, if exist
            string oldFile = FileHelper.GetFile(oldFileName, subdirectory);
            if (File.Exists(oldFile))
                File.Delete(oldFile);
        }

        private static string ProcessFile(string oldFileName, string newFile, string subdirectory, string rename)
        {
            //check if file is changed
            if (oldFileName == newFile) return oldFileName;
            //upload new file, if exist
            string file = null;
            if (!string.IsNullOrEmpty(newFile))
            {
                rename = rename + "_" + DateTime.Now.ToString("MM-dd-yyyy");
                file = FileHelper.Upload(newFile, subdirectory, rename, FileHelper.FileNameRenameMode.FullRename);
            }
            //delete old file, if exist
            string oldFile = FileHelper.GetFile(oldFileName, subdirectory);
            if (File.Exists(oldFile))
                File.Delete(oldFile);
            return Path.GetFileName(file);
        }

        public static async Task AddUpdateCategoryAsync(CategoryViewModel categoryVm)
        {
            using (var context = new BillsModel())
            {
                var category = await context.categories.FirstOrDefaultAsync(i => i.Id == categoryVm.Id);
                if (category == null)
                {
                    category = new category();
                    context.categories.Add(category);
                }
                category.CategoryName = categoryVm.CategoryName;
                category.Description = categoryVm.Description;
                category.IsActive = categoryVm.IsActive.ToLong();
                await context.SaveChangesAsync();
                categoryVm.Id = (int)category.Id;
            }
        }

        public static async Task DeleteBillPaymentAsync(BillPaymentViewModel paymentVm)
        {
            using (var context = new BillsModel())
            {
                var payment = await context.bill_payment.FirstOrDefaultAsync(i => i.Id == paymentVm.Id);
                if (payment != null)
                {
                    context.bill_payment.Remove(payment);
                    DeleteFile(payment.BillFile, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY);
                    DeleteFile(payment.ReceiptFile, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task<IEnumerable<BillPeriodViewModel>> GetBillPeriodListAsync()
        {
            using (var context = new BillsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from p in context.bill_period
                            join cat in context.categories on p.CategoryId equals cat.Id
                            join incl in context.period_inclusion on p.Id equals incl.PeriodId into periodInclusion
                            from p_incl in periodInclusion.DefaultIfEmpty()
                            select new
                            {
                                Id = p.Id,
                                Period = p,
                                Category = cat,
                                Inclusion = p_incl
                            };
                var list = await query.ToListAsync();
                return list.GroupBy(i => i.Id).Select(g =>
                {
                    var vm = new BillPeriodViewModel();
                    bill_period period = g.First().Period;
                    category cat = g.First().Category;
                    IEnumerable<period_inclusion> inclusionList = g.Where(i => i.Inclusion != null).Select(i => i.Inclusion);
                    //bill contract info
                    vm.Id = (int)period.Id;
                    vm.BillerName = period.BillerName;
                    vm.Category = new CategoryViewModel(cat);
                    vm.Description = period.Description;
                    vm.AccountName = period.AccountName;
                    vm.AccountNumber = period.AccountNumber;
                    vm.ContactPerson = period.ContactPerson;
                    vm.ContactNumber = period.ContactNumber;
                    vm.Address = period.Address;
                    vm.UnitNumber = period.UnitNumber;
                    vm.PhoneNumber = period.PhoneNumber;
                    vm.ContractAmount = period.ContractAmount;
                    vm.ContractExpiration = period.ContractExpiration.ToUnixDate();
                    vm.ContractFile = period.ContractFile;
                    vm.ReferenceLink = period.ReferenceLink;
                    vm.Username = period.Username;
                    vm.Password = period.Password;
                    vm.BillDescription = period.BillDescription;
                    vm.BillInstructions = period.BillInstructions;
                    vm.VideoInstructionsFile = period.VideoInstructionsFile;
                    vm.IsVat = period.IsVat.ToBool();
                    vm.TaxType = period.TaxType.ToEnum<BillTaxType>();
                    vm.TaxRate = period.TaxRate;
                    vm.TinNumber = period.TinNumber;
                    //bill period info
                    vm.PeriodType = period.PeriodType.ToEnum<BillPeriodType>();
                    vm.DueDateStart = period.DueDateStart.ToUnixDate();
                    vm.DueDateEnd = period.DueDateEnd.ToUnixDate();
                    vm.DueMonth = (int?)period.DueMonth;
                    vm.DueDays = (int?)period.DueDays;
                    vm.IsActive = period.IsActive.ToBool();
                    //inclusion
                    if (inclusionList != null && inclusionList.Count() > 0)
                    {
                        //always ensure that inclusion list is complete (months: 12, quarters: 4)
                        //if missing, default to included
                        //month
                        var monthInclusionList = inclusionList.Where(i => i.InclusionType.ToEnum<BillPeriodType>() == BillPeriodType.Monthly);
                        if (monthInclusionList != null && monthInclusionList.Count() > 0)
                        {
                            foreach (var monthInclVm in vm.MonthInclusionList)
                            {
                                period_inclusion incl = monthInclusionList.FirstOrDefault(i => i.Value == monthInclVm.Value);
                                if (incl != null)
                                {
                                    monthInclVm.Id = (int)incl.Id;
                                    monthInclVm.IsIncluded = incl.IsIncluded.ToBool();
                                }
                            }
                        }
                        //quarter
                        var quarterInclusionList = inclusionList.Where(i => i.InclusionType.ToEnum<BillPeriodType>() == BillPeriodType.EndOfQuarter);
                        if (quarterInclusionList != null && quarterInclusionList.Count() > 0)
                        {
                            foreach (var quarterInclVm in vm.QuarterInclusionList)
                            {
                                period_inclusion incl = quarterInclusionList.FirstOrDefault(i => i.Value == quarterInclVm.Value);
                                if (incl != null)
                                {
                                    quarterInclVm.Id = (int)incl.Id;
                                    quarterInclVm.IsIncluded = incl.IsIncluded.ToBool();
                                }
                            }
                        }
                    }

                    return vm;
                });
            }
        }

        public static async Task<IEnumerable<BillPeriodViewModel>> GetBillPeriodSimpleListAsync()
        {
            using (var context = new BillsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from p in context.bill_period
                            join cat in context.categories on p.CategoryId equals cat.Id
                            select new
                            {
                                Id = p.Id,
                                Period = p,
                                Category = cat
                            };
                var list = await query.ToListAsync();
                return list.GroupBy(i => i.Id).Select(g =>
                {
                    var vm = new BillPeriodViewModel();
                    bill_period period = g.First().Period;
                    category cat = g.First().Category;
                    //period, only include basic info, used for filtering
                    vm.Id = (int)period.Id;
                    vm.BillerName = period.BillerName;
                    vm.Category = new CategoryViewModel(cat);
                    vm.BillDescription = period.BillDescription;
                    vm.IsVat = period.IsVat.ToBool();
                    vm.IsActive = period.IsActive.ToBool();
                    
                    return vm;
                });
            }
        }

        public static async Task<IEnumerable<CategoryViewModel>> GetCategoryListAsync()
        {
            using (var context = new BillsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from cat in context.categories
                            select cat;
                var list = await query.ToListAsync();
                return list.Select(i => new CategoryViewModel(i))
                    .OrderBy(i => i.CategoryName);
            }
        }

        public static async Task<IEnumerable<BillPaymentViewModel>> GetBillPaymentListAsync()
        {
            using (var context = new BillsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from payment in context.bill_payment
                            join period in context.bill_period on payment.PeriodId equals period.Id
                            join cat in context.categories on period.CategoryId equals cat.Id
                            select new
                            {
                                Payment = payment,
                                Period = period,
                                Category = cat
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new BillPaymentViewModel()
                {
                    Id = (int)i.Payment.Id,
                    PeriodDate = i.Payment.PeriodDate.ToUnixDate(),
                    PaymentDate = i.Payment.PaymentDate.ToUnixDate(),
                    Amount = i.Payment.Amount,
                    BillFileName = i.Payment.BillFile,
                    ReceiptFileName = i.Payment.ReceiptFile,
                    Notes = i.Payment.Notes,
                    PeriodFrom = i.Payment.PeriodFrom.ToUnixDate(),
                    PeriodTo = i.Payment.PeriodTo.ToUnixDate(),
                    BillAmount = i.Payment.BillAmount,
                    BillReferenceNumber = i.Payment.BillReferenceNumber,
                    PaymentType = i.Payment.PaymentType,
                    VatAmount = i.Payment.VatAmount,
                    NetVatAmount = i.Payment.NetVatAmount,
                    Period = new BillPeriodViewModel()
                    {
                        Id = (int)i.Period.Id,
                        BillerName = i.Period.BillerName,
                        Category = new CategoryViewModel(i.Category),
                        BillDescription = i.Period.BillDescription,
                        IsVat = i.Period.IsVat.ToBool(),
                        TaxType = i.Period.TaxType.ToEnum<BillTaxType>(),
                        TaxRate = i.Period.TaxRate,
                        TinNumber = i.Period.TinNumber,
                        IsActive = i.Period.IsActive.ToBool()
                    }
                }).OrderBy(i => i.PeriodDate);
            }
        }

        public static async Task<IEnumerable<BillPaymentViewModel>> GetBillPaymentListAsync(int periodId)
        {
            using (var context = new BillsModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from payment in context.bill_payment
                            join period in context.bill_period on payment.PeriodId equals period.Id
                            join cat in context.categories on period.CategoryId equals cat.Id
                            where period.Id == periodId
                            orderby payment.PaymentDate descending
                            select new
                            {
                                Payment = payment,
                                Period = period,
                                Category = cat
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new BillPaymentViewModel()
                {
                    Id = (int)i.Payment.Id,
                    PeriodDate = i.Payment.PeriodDate.ToUnixDate(),
                    PaymentDate = i.Payment.PaymentDate.ToUnixDate(),
                    Amount = i.Payment.Amount,
                    BillFileName = i.Payment.BillFile,
                    ReceiptFileName = i.Payment.ReceiptFile,
                    Notes = i.Payment.Notes,
                    PeriodFrom = i.Payment.PeriodFrom.ToUnixDate(),
                    PeriodTo = i.Payment.PeriodTo.ToUnixDate(),
                    BillAmount = i.Payment.BillAmount,
                    BillReferenceNumber = i.Payment.BillReferenceNumber,
                    PaymentType = i.Payment.PaymentType,
                    Period = new BillPeriodViewModel()
                    {
                        Id = (int)i.Period.Id,
                        BillerName = i.Period.BillerName,
                        Category = new CategoryViewModel(i.Category),
                        BillDescription = i.Period.BillDescription,
                        IsActive = i.Period.IsActive.ToBool()
                    }
                }).OrderBy(i => i.PeriodDate);
            }
        }

        public static void ComputeVat(decimal amount, decimal rate, out decimal vat, out decimal netvat)
        {
            decimal tax = (rate / 100) + 1;
            netvat = Math.Round(amount / tax, 2);
            vat = Math.Round(amount - netvat, 2);
        }

        public static async Task SetMissingBillsCountAsync(List<BillPeriodViewModel> billList)
        {
            var tasks = billList.Select(l => Task.Run(async () =>
            {
                using (var context = new BillsModel())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    var query = from pay in context.bill_payment
                                where pay.PeriodId == l.Id
                                select new { pay.PeriodDate, pay.PeriodFrom, pay.PeriodTo };
                    var list = await query.ToListAsync();
                    var paymentList = list.Select(i => new BillPaymentViewModel() { PeriodDate = i.PeriodDate.ToUnixDate(), PeriodFrom = i.PeriodFrom.ToUnixDate(), PeriodTo = i.PeriodTo.ToUnixDate() });
                    //get period dates to be checked, as of today
                    var dates = l.GetPeriodDatesByYear(DateTime.Now.Year).Where(x => x.Date <= DateTime.Now.Date);
                    //get missing bills by PeriodDate
                    l.MissingBillsByDueDate = dates.Where(x => !paymentList.Any(i => i.PeriodFrom.Date >= x && x <= i.PeriodTo.Date)).ToList();
                }
            }));
            await Task.WhenAll(tasks);
        }

        public static async Task SetMissingBillsAmount(List<BillPeriodViewModel> billList)
        {
            var tasks = billList.Select(l => Task.Run(async () =>
            {
                using (var context = new BillsModel())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    var query = from pay in context.bill_payment
                                where pay.PeriodId == l.Id
                                select new
                                {
                                    pay.PeriodDate,
                                    HasBill = pay.BillAmount != 0 && (pay.BillFile != null && pay.BillFile != ""),
                                    HasReceipt = pay.Amount != 0 && (pay.ReceiptFile != null && pay.ReceiptFile != ""),
                                };
                    var list = await query.ToListAsync();
                    l.MissingBillsAmountFile = list.Where(i => !i.HasBill)
                                                    .Select(i => i.PeriodDate.ToUnixDate())
                                                    .ToList();
                    l.MissingReceiptAmountFile = list.Where(i => !i.HasReceipt)
                                                    .Select(i => i.PeriodDate.ToUnixDate())
                                                    .ToList();
                }
            }));
            await Task.WhenAll(tasks);
        }
    }
}
