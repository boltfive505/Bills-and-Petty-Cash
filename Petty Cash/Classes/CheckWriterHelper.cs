using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Petty_Cash.Objects.CheckWriter;
using Petty_Cash.Model;

namespace Petty_Cash.Classes
{
    public static class CheckWriterHelper
    {
        public static async Task<IEnumerable<CheckWriterBankViewModel>> GetBankListAsync(bool includeInactive = true)
        {
            using (var context = new PettyCashModel())
            {
                var query = from bk in context.bank
                            where includeInactive ? true : bk.IsActive == 1
                            select bk;
                var list = await query.ToListAsync();
                return list.Select(i => new CheckWriterBankViewModel(i));
            }
        }

        public static async Task<IEnumerable<CheckWriterAccountViewModel>> GetAccountsListAync(bool includeInactive = true)
        {
            using (var context = new PettyCashModel())
            {
                var query = from acc in context.bank_account
                            join bnk in context.bank on acc.BankId equals bnk.Id
                            where includeInactive ? true : acc.IsActive == 1 && bnk.IsActive == 1
                            select new { Account = acc, Bank = bnk };
                var list = await query.ToListAsync();
                return list.Select(i => new CheckWriterAccountViewModel(i.Account, i.Bank));
            }
        }

        public static async Task<IEnumerable<CheckWriterCheckViewModel>> GetCheckListAync(bool includeInactive = true)
        {
            using (var context = new PettyCashModel())
            {
                var query = from chk in context.bank_check
                            join acc in context.bank_account on chk.AccountId equals acc.Id
                            join bnk in context.bank on acc.BankId equals bnk.Id
                            join vch in context.voucher on chk.Id equals vch.CheckId into checkVoucher
                            from chk_vch in checkVoucher.DefaultIfEmpty()
                            where includeInactive ? true : acc.IsActive == 1 && bnk.IsActive == 1
                            select new { Check = chk, Account = acc, Bank = bnk, Voucher = chk_vch };
                var list = await query.ToListAsync();
                return list.Select(i => new CheckWriterCheckViewModel(i.Check, i.Voucher, i.Account, i.Bank));
            }
        }

        public static async Task AddBankAsync(CheckWriterBankViewModel bankVm)
        {
            using (var context = new PettyCashModel())
            {
                var bank = await context.bank.FirstOrDefaultAsync(i => i.Id == bankVm.Id);
                if (bank == null)
                {
                    bank = new bank();
                    context.bank.Add(bank);
                }
                bank.BankName = bankVm.BankName;
                bank.Branch = bankVm.Branch;
                bank.Description = bankVm.Description;
                bank.IsActive = bankVm.IsActive.ToLong();
                await context.SaveChangesAsync();

                bankVm.Id = (int)bank.Id;
            }
        }

        public static async Task AddAccountAsync(CheckWriterAccountViewModel accountVm)
        {
            using (var context = new PettyCashModel())
            {
                var account = await context.bank_account.FirstOrDefaultAsync(i => i.Id == accountVm.Id);
                if (account == null)
                {
                    account = new bank_account();
                    context.bank_account.Add(account);
                }
                account.AccountName = accountVm.AccountName;
                account.AccountNumber = accountVm.AccountNumber;
                account.AccountType = accountVm.AccountType.ToString();
                account.BankId = accountVm.Bank.Id;
                account.Description = accountVm.Description;
                account.ContactPerson = accountVm.ContactPerson;
                account.ContactNumber = accountVm.ContactNumber;
                account.Purpose = accountVm.Purpose;
                account.IsActive = accountVm.IsActive.ToLong();
                await context.SaveChangesAsync();

                accountVm.Id = (int)account.Id;
            }
        }

        public static async Task AddCheckAsync(CheckWriterCheckViewModel checkVm)
        {
            using (var context = new PettyCashModel())
            {
                var check = await context.bank_check.FirstOrDefaultAsync(i => i.Id == checkVm.Id);
                if (check == null)
                {
                    check = new bank_check();
                    context.bank_check.Add(check);
                }
                check.AccountId = checkVm.Account.Id;
                check.Amount = checkVm.Amount;
                check.CheckNumber = checkVm.CheckNumber;
                check.PayeeName = checkVm.PayeeName;
                check.CheckDate = checkVm.CheckDate.ToUnixLong();
                check.Description = checkVm.Description;
                check.UpdatedDate = checkVm.UpdatedDate.ToUnixLong();
                check.PrintedDate = checkVm.PrintedDate.ToUnixLong();
                check.PayToBillName = checkVm.PayToBillName;
                check.AlreadyUsed = (int)checkVm.AlreadyUsed.ToLong();
                await context.SaveChangesAsync();

                checkVm.Id = (int)check.Id;
            }
        }

        public static async Task SetCheckAsUsed(CheckWriterCheckViewModel checkVm)
        {
            using (var context = new PettyCashModel())
            {
                var check = await context.bank_check.FirstOrDefaultAsync(i => i.Id == checkVm.Id);
                if (check != null)
                {
                    check.AlreadyUsed = true.ToLong(); //(int)checkVm.AlreadyUsed.ToLong();
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
