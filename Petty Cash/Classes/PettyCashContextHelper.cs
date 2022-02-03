using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.IO;
using Petty_Cash.Model;
using Petty_Cash.Objects;

namespace Petty_Cash.Classes
{
    public static class PettyCashContextHelper
    {
        public const string PAYEMNT_FILE_SUBDIRECTORY = "uploads\\petty cash\\payment";

        public static async Task AddUpdatePettyCashAsync(PettyCashViewModel pettyVm, bool closeVoucher)
        {
            try
            {
                using (var context = new PettyCashModel())
                {
                    var petty = await context.transaction.FirstOrDefaultAsync(i => i.Id == pettyVm.Id);
                    if (petty == null)
                    {
                        petty = new transaction();
                        context.transaction.Add(petty);
                    }
                    //petty cash
                    petty.PayeeId = pettyVm.Payee?.Id;
                    petty.VoucherId = pettyVm.Voucher?.Id;
                    petty.CategoryId = pettyVm.Category?.Id;
                    petty.TransactionDate = pettyVm.TransactionDate.ToUnixLong();
                    petty.AmountIn = pettyVm.AmountIn;
                    petty.AmountOut = pettyVm.AmountOut;
                    petty.AmountReturn = pettyVm.AmountReturn;
                    petty.Purpose = pettyVm.Purpose;
                    petty.PaymentFile = ProcessPaymentFile(petty.PaymentFile, pettyVm.PaymentFile, string.Format("payment_{0}_{1}", pettyVm.Payee?.PayeeName, pettyVm.Category?.CategoryName));
                    petty.CompanyId = pettyVm.Company?.Id;
                    petty.IsVat = pettyVm.IsVat.ToLong();
                    //voucher
                    if (closeVoucher && pettyVm.Voucher != null)
                    {
                        var voucher = await context.voucher.FirstOrDefaultAsync(i => i.Id == pettyVm.Voucher.Id);
                        if (voucher != null)
                        {
                            voucher.IsLiquidated = true.ToLong();
                        }
                    }

                    await context.SaveChangesAsync();
                    pettyVm.Id = (int)petty.Id;
                    pettyVm.PaymentFile = petty.PaymentFile;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private static string ProcessPaymentFile(string oldFileName, string newFile, string rename)
        {
            //check if file is changed
            if (oldFileName == newFile) return oldFileName;
            //upload new file, if exist
            rename = rename + "_" + DateTime.Now.ToString("MM-dd-yyyy");
            string file = FileHelper.Upload(newFile, PettyCashContextHelper.PAYEMNT_FILE_SUBDIRECTORY, rename, FileHelper.FileNameRenameMode.FullRename);
            //delete old file, if exist
            string oldFile = FileHelper.GetFile(oldFileName, PettyCashContextHelper.PAYEMNT_FILE_SUBDIRECTORY);
            if (File.Exists(oldFile))
                File.Delete(oldFile);
            return Path.GetFileName(file);
        }

        public static async Task ClearVoucherTransaction(VoucherViewModel voucherVm)
        {
            using (var context = new PettyCashModel())
            {
                var voucher = await context.voucher.FirstOrDefaultAsync(i => i.Id == voucherVm.Id);
                if (voucher != null)
                {
                    voucher.IsLiquidated = false.ToLong(); //re-open voucher
                    //clear all transactions for this voucher
                    context.transaction.RemoveRange(context.transaction.Where(i => i.VoucherId == voucherVm.Id));
                    await context.SaveChangesAsync();

                    voucherVm.ExpenseAmount = 0;
                    voucherVm.ReturnAmount = 0;
                    voucherVm.IsLiquidated = false;
                }
            }
        }

        public static async Task DeletePettyCashAsync(PettyCashViewModel pettyCashVm)
        {
            using (var context = new PettyCashModel())
            {
                var pettyCash = await context.transaction.FirstOrDefaultAsync(i => i.Id == pettyCashVm.Id);
                if (pettyCash != null)
                {
                    context.transaction.Remove(pettyCash);
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task AddUpdateCategoryAsync(CategoryViewModel categoryVm)
        {
            using (var context = new PettyCashModel())
            {
                var category = await context.category.FirstOrDefaultAsync(i => i.Id == categoryVm.Id);
                if (category == null)
                {
                    category = new category();
                    context.category.Add(category);
                }
                category.CategoryName = categoryVm.CategoryName;
                category.Description = categoryVm.Description;
                await context.SaveChangesAsync();
                categoryVm.Id = (int)category.Id;
            }
        }

        public static async Task AddUpdatePayeeAsync(PayeeViewModel payeeVm)
        {
            using (var context = new PettyCashModel())
            {
                var payee = await context.payee.FirstOrDefaultAsync(i => i.Id == payeeVm.Id);
                if (payee == null)
                {
                    payee = new payee();
                    context.payee.Add(payee);
                }
                payee.PayeeName = payeeVm.PayeeName;
                payee.Description = payeeVm.Description;
                payee.IsActive = (int)payeeVm.IsActive.ToLong();
                await context.SaveChangesAsync();
                payeeVm.Id = (int)payee.Id;
            }
        }

        public static async Task AddUpdateCompanyAsync(CompanyViewModel companyVm)
        {
            using (var context = new PettyCashModel())
            {
                var company = await context.company.FirstOrDefaultAsync(i => i.Id == companyVm.Id);
                if (company == null)
                {
                    company = new company();
                    context.company.Add(company);
                }
                company.CompanyName = companyVm.CompanyName;
                company.Address = companyVm.Address;
                company.TinNumber = companyVm.TinNumber.Replace("-", ""); //remove dash on tin #
                await context.SaveChangesAsync();
                companyVm.Id = (int)company.Id;
            }
        }

        public static async Task AddUpdateDepartmentAsync(DepartmentViewModel departmentVm)
        {
            using (var context = new PettyCashModel())
            {
                var department = await context.department.FirstOrDefaultAsync(i => i.Id == departmentVm.Id);
                if (department == null)
                {
                    department = new department();
                    context.department.Add(department);
                }
                department.DepartmentName = departmentVm.DepartmentName;
                department.JobDescription = departmentVm.JobDescription;
                await context.SaveChangesAsync();
                departmentVm.Id = (int)department.Id;
            }
        }

        public static async Task AddUpdateVoucherAsync(VoucherViewModel voucherVm)
        {
            using (var context = new PettyCashModel())
            {
                var voucher = await context.voucher.FirstOrDefaultAsync(i => i.Id == voucherVm.Id);
                if (voucher == null)
                {
                    voucher = new voucher();
                    context.voucher.Add(voucher);
                }
                voucher.PayeeId = voucherVm.Payee?.Id;
                voucher.VoucherDate = voucherVm.VoucherDate.ToUnixLong();
                voucher.AmountReceived = voucherVm.AmountReceived;
                voucher.AmountReplenished = voucherVm.AmountReplenished;
                voucher.IsLiquidated = voucherVm.IsLiquidated.ToLong();
                voucher.Particulars = voucherVm.Particulars;
                voucher.CheckId = voucherVm.Check?.Id;
                await context.SaveChangesAsync();
                voucherVm.Id = (int)voucher.Id;
            }
        }

        public static async Task<IEnumerable<PettyCashViewModel>> GetPettyCashListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from tr in context.transaction
                            join py in context.payee on tr.PayeeId equals py.Id into transaction_payee
                            from tr_py in transaction_payee.DefaultIfEmpty()
                            join cat in context.category on tr.CategoryId equals cat.Id into transaction_category
                            from tr_cat in transaction_category.DefaultIfEmpty()
                            join comp in context.company on tr.CompanyId equals comp.Id into transaction_company
                            from tr_comp in transaction_company.DefaultIfEmpty()
                            join vch in context.voucher on tr.VoucherId equals vch.Id into transaction_voucher
                            from tr_vch in transaction_voucher.DefaultIfEmpty()
                            select new
                            {
                                Transaction = tr,
                                Payee = tr_py,
                                Category = tr_cat,
                                Company = tr_comp,
                                Voucher = tr_vch
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new PettyCashViewModel()
                {
                    Id = (int)i.Transaction.Id,
                    Voucher = i.Voucher == null ? null : new VoucherViewModel(i.Voucher, i.Payee),
                    Payee = new PayeeViewModel(i.Payee),
                    Category = i.Category == null ? null : new CategoryViewModel(i.Category),
                    TransactionDate = i.Transaction.TransactionDate.ToUnixDate(),
                    AmountIn = i.Transaction.AmountIn,
                    AmountOut = i.Transaction.AmountOut,
                    AmountReturn = i.Transaction.AmountReturn,
                    Purpose = i.Transaction.Purpose,
                    PaymentFile = i.Transaction.PaymentFile,
                    Company = i.Company == null ? null : new CompanyViewModel(i.Company),
                    IsVat = i.Transaction.IsVat.ToBool()
                }).OrderByDescending(i => i.TransactionDate);
            }
        }

        public static async Task<IEnumerable<PettyCashViewModel>> GetPettyCashAmountListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from tr in context.transaction
                            join py in context.payee on tr.PayeeId equals py.Id into transaction_payee
                            from tr_py in transaction_payee.DefaultIfEmpty()
                            select new
                            {
                                tr.AmountIn,
                                tr.AmountOut,
                                tr.AmountReturn
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new PettyCashViewModel()
                {
                    AmountIn = i.AmountIn,
                    AmountOut = i.AmountOut,
                    AmountReturn = i.AmountReturn,
                });
            }
        }

        public static async Task<IEnumerable<PettyCashViewModel>> GetPettyCashListFromVoucherIdAsync(int voucherId)
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from tr in context.transaction
                            join py in context.payee on tr.PayeeId equals py.Id into transaction_payee
                            from tr_py in transaction_payee.DefaultIfEmpty()
                            join cat in context.category on tr.CategoryId equals cat.Id into transaction_category
                            from tr_cat in transaction_category.DefaultIfEmpty()
                            join comp in context.company on tr.CompanyId equals comp.Id into transaction_company
                            from tr_comp in transaction_company.DefaultIfEmpty()
                            join vch in context.voucher on tr.VoucherId equals vch.Id into transaction_voucher
                            from tr_vch in transaction_voucher.DefaultIfEmpty()
                            where tr_vch != null && tr_vch.Id == voucherId
                            select new
                            {
                                Transaction = tr,
                                Payee = tr_py,
                                Category = tr_cat,
                                Company = tr_comp,
                                Voucher = tr_vch
                            };
                var list = await query.ToListAsync();
                return list.Select(i => new PettyCashViewModel()
                {
                    Id = (int)i.Transaction.Id,
                    Voucher = new VoucherViewModel(i.Voucher, i.Payee),
                    Payee = new PayeeViewModel(i.Payee),
                    Category = new CategoryViewModel(i.Category),
                    TransactionDate = i.Transaction.TransactionDate.ToUnixDate(),
                    AmountIn = i.Transaction.AmountIn,
                    AmountOut = i.Transaction.AmountOut,
                    AmountReturn = i.Transaction.AmountReturn,
                    Purpose = i.Transaction.Purpose,
                    PaymentFile = i.Transaction.PaymentFile,
                    Company = new CompanyViewModel(i.Company),
                    IsVat = i.Transaction.IsVat.ToBool()
                }).OrderByDescending(i => i.TransactionDate);
            }
        }

        public static async Task<IEnumerable<CategoryViewModel>> GetCategoryListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from cat in context.category
                            select cat;
                var list = await query.ToListAsync();
                return list.Select(i => new CategoryViewModel(i))
                    .OrderBy(i => i.CategoryName);
            }
        }

        public static async Task<IEnumerable<DepartmentViewModel>> GetDepartmentListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from dept in context.department
                            select dept;
                var list = await query.ToListAsync();
                return list.Select(i => new DepartmentViewModel(i));
            }
        }

        public static async Task<IEnumerable<PayeeViewModel>> GetPayeeListAsync(bool includeInactive)
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from py in context.payee
                            select py;
                var list = await query.ToListAsync();
                return list.Where(i =>
                {
                    if (!includeInactive)
                        return i.IsActive == 1;
                    return true;
                })
                .Select(i => new PayeeViewModel(i))
                .OrderBy(i => i.PayeeName);
            }
        }

        public static async Task GetPayeeBalanceAsync(IList<PayeeViewModel> list)
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var voucherQuery = from vch in context.voucher
                                   where vch.PayeeId != null && vch.IsLiquidated == 0 //get open vouchers
                                   select new { vch.PayeeId, vch.AmountReceived };
                //var transactionQuery = from tr in context.transaction
                //                       where tr.PayeeId != null
                //                       select new { tr.PayeeId, tr.AmountOut, tr.AmountReturn };

                var voucherTask = voucherQuery.ToListAsync();
                //var transactionTask = transactionQuery.ToListAsync();
                await Task.WhenAll(voucherTask);

                foreach (var l in list)
                {
                    var vlist = voucherTask.Result.Where(i => i.PayeeId == l.Id);
                    //var tlist = transactionTask.Result.Where(i => i.PayeeId == l.Id);
                    l.Amount = vlist.Sum(i => (i.AmountReceived ?? 0));
                    //l.Amount = (vlist.Sum(v => v.AmountReceived ?? 0) + tlist.Sum(t => t.AmountReturn ?? 0)) - tlist.Sum(t => t.AmountOut ?? 0);
                }
            }
        }

        public static async Task<IEnumerable<CompanyViewModel>> GetCompanyListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from comp in context.company
                            select comp;
                var list = await query.ToListAsync();
                return list.Select(i => new CompanyViewModel(i))
                    .OrderBy(i => i.CompanyName);
            }
        }

        public static async Task<IEnumerable<VoucherViewModel>> GetVoucherListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var transactionQuery = from tr in context.transaction
                                       where tr.VoucherId != null
                                       select tr;
                var voucherQuery = from vch in context.voucher
                                   join py in context.payee on vch.PayeeId equals py.Id into payeeVoucher
                                   from py_vch in payeeVoucher.DefaultIfEmpty()
                                   join chk in context.bank_check on vch.CheckId equals chk.Id into checkVoucher
                                   from chk_vch in checkVoucher.DefaultIfEmpty()
                                   select new
                                   {
                                       Voucher = vch,
                                       Payee = py_vch,
                                       Check = chk_vch
                                   };
                var query = from vch in voucherQuery
                            join tr in transactionQuery on vch.Voucher.Id equals tr.VoucherId into voucher_transaction
                            from vch_tr in voucher_transaction.DefaultIfEmpty()
                            select new { Voucher = vch.Voucher, Payee = vch.Payee, Check = vch.Check, Transaction = vch_tr };
                var list = await query.ToListAsync();
                return list.GroupBy(i => i.Voucher.Id)
                    .Where(g =>
                    {
                        var payee = g.First().Payee;
                        if (payee != null)
                            return payee.IsActive == 1;
                        return true;
                    })
                    .Select(g =>
                    {
                        VoucherViewModel voucherVm = new VoucherViewModel(g.First().Voucher, g.First().Payee, g.First().Check);
                        var transactions = g.Where(i => i.Transaction != null).Select(i => i.Transaction);
                        if (transactions != null && transactions.Count() > 0)
                        {
                            voucherVm.ExpenseAmount = transactions.Sum(i => i.AmountOut ?? 0);
                            voucherVm.ReturnAmount = transactions.Sum(i => i.AmountReturn ?? 0);
                        }
                        return voucherVm;
                    });
            }
        }

        public static async Task<IEnumerable<VoucherViewModel>> GetOpenVoucherListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var transactionQuery = from tr in context.transaction
                                       where tr.VoucherId != null && tr.AmountIn == null && tr.AmountOut != null
                                       select tr;
                var voucherQuery = from vch in context.voucher
                                   join py in context.payee on vch.PayeeId equals py.Id
                                   select new
                                   {
                                       Voucher = vch,
                                       Payee = py
                                   };
                var query = from vch in voucherQuery
                            join tr in transactionQuery on vch.Voucher.Id equals tr.VoucherId into voucher_transaction
                            from vch_tr in voucher_transaction.DefaultIfEmpty()
                            where vch.Voucher.IsLiquidated == 0
                            select new { Voucher = vch.Voucher, Payee = vch.Payee, Transaction = vch_tr };
                var list = await query.ToListAsync();
                return list.GroupBy(i => i.Voucher.Id)
                    .Where(g =>
                    {
                        var payee = g.First().Payee;
                        if (payee != null)
                            return payee.IsActive == 1;
                        return true;
                    })
                    .Select(g =>
                    {
                        VoucherViewModel voucherVm = new VoucherViewModel(g.First().Voucher, g.First().Payee);
                        var transactions = g.Where(i => i.Transaction != null).Select(i => i.Transaction);
                        if (transactions != null && transactions.Count() > 0)
                        {
                            voucherVm.ExpenseAmount = transactions.Sum(i => i.AmountOut ?? 0);
                        }
                        return voucherVm;
                    });
            }
        }

        public static async Task<IEnumerable<VoucherViewModel>> GetReplenishVoucherAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from vch in context.voucher
                            where vch.AmountReplenished != null
                            select vch;
                var list = await query.ToListAsync();
                return list.Select(i => new VoucherViewModel(i, null));
            }
        }

        public static async Task<IEnumerable<VoucherViewModel>> GetVoucherSimpleListAsync()
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var query = from vch in context.voucher
                            join py in context.payee on vch.PayeeId equals py.Id
                            select new 
                            {
                                Voucher = vch, 
                                Payee = py 
                            };
                var list = await query.ToListAsync();
                return list.Where(i =>
                {
                    if (i.Payee != null)
                        return i.Payee.IsActive == 1;
                    return true;
                }).Select(i => new VoucherViewModel(i.Voucher, i.Payee));
            }
        }

        public static async Task ReloadVoucherInfoAsync(VoucherViewModel voucherVm)
        {
            using (var context = new PettyCashModel())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                var voucher = await context.voucher.FirstOrDefaultAsync(i => i.Id == voucherVm.Id);
                if (voucher != null)
                {
                    var transactionQuery = from tr in context.transaction
                                           where tr.VoucherId == voucher.Id && tr.AmountOut != null
                                           select tr;
                    var list = await transactionQuery.ToListAsync();
                    voucherVm.ExpenseAmount = list.Sum(i => i.AmountOut ?? 0);
                }
            }
        }
    }
}
