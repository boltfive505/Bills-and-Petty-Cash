using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using Microsoft.Win32;
using System.IO;
using Petty_Cash.Objects;
using Petty_Cash.Objects.Bills;
using System.Collections;
using System.Globalization;

namespace Petty_Cash.Classes
{
    public static class ExcelHelper
    {
        public static void ExportPettyCashList(IEnumerable<PettyCashViewModel> list)
        {
            //prepare fields
            FieldExpressionCollection<PettyCashViewModel> expressionList = new FieldExpressionCollection<PettyCashViewModel>();
            expressionList.Add(i => i.Voucher.Id, "Voucher Id");
            expressionList.Add(i => i.TransactionDate, "Transaction Date");
            expressionList.Add(i => i.Category.CategoryName, "Category");
            expressionList.Add(i => i.Payee.PayeeName, "Payee");
            expressionList.Add(i => i.AmountOut, "Cash-Out");
            expressionList.Add(i => i.Purpose);
            expressionList.Add(i => i.Company.TinNumber, "TIN #");
            expressionList.Add(i => i.Company.CompanyName, "Company");
            expressionList.Add(i => i.Company.Address, "Address");
            expressionList.Add(i => i.VatAmount, "VAT");
            expressionList.Add(i => i.NetVatAmount, "Net of VAT");
            expressionList.Add(i => i.NonVatAmount, "Non-VAT");

            //prepare excel
            byte[] excelData = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("petty cash");

                    //create header
                    IRow headerRow = sheet.CreateRow(0);
                    for (int i = 0; i < expressionList.Count; i++)
                        headerRow.CreateCell(i).SetCellValue(expressionList[i].GetFieldName());
                    //create rows
                    int r = 1;
                    foreach (var i in list)
                    {
                        IRow row = sheet.CreateRow(r);
                        for (int a = 0; a < expressionList.Count; a++)
                            row.CreateCell(a).SetCellObjectValue(expressionList[a].GetValue(i));
                        r++;
                    }

                    workbook.Write(ms);
                    excelData = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logs.WriteException(ex);
                System.Windows.MessageBox.Show("An error occured while processing the file.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            SaveExcelData(excelData, "Save Petty Cash List", string.Format("petty cash_{0:yyyy-MM-dd}", DateTime.Now));
        }

        public static void ExportBillPaymentList(IEnumerable<BillPaymentViewModel> list)
        {
            //prepare fields
            FieldExpressionCollection<BillPaymentViewModel> expressionList = new FieldExpressionCollection<BillPaymentViewModel>();
            expressionList.Add(i => i.PeriodDate, "Due Date");
            expressionList.Add(i => i.Period.Category.CategoryName, "Category");
            expressionList.Add(i => i.Period.BillerName, "Biller Name");
            expressionList.Add(i => i.Period.TinNumber, "TIN #");
            expressionList.Add(i => i.Period.BillDescription, "Description");
            expressionList.Add(i => i.PeriodFrom, "Period From");
            expressionList.Add(i => i.PeriodTo, "Period To");
            expressionList.Add(i => i.VatAmount, i => i.Period.TaxType == BillTaxType.Withholding_Tax, "WHT");
            expressionList.Add(i => i.VatAmount, i => i.Period.TaxType == BillTaxType.Vat, "VAT (12%)");
            expressionList.Add(i => i.NetVatAmount, "Net");
            expressionList.Add(i => i.BillAmount, "Bill Amount");
            expressionList.Add(i => i.Amount, "Paid Amount");
            expressionList.Add(i => i.PaymentDate, "Payment Date");

            //prepare excel
            byte[] excelData = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("bills payment");

                    //create header
                    IRow headerRow = sheet.CreateRow(0);
                    for (int i = 0; i < expressionList.Count; i++)
                        headerRow.CreateCell(i).SetCellValue(expressionList[i].GetFieldName());
                    //create rows
                    int r = 1;
                    foreach (var i in list)
                    {
                        IRow row = sheet.CreateRow(r);
                        for (int a = 0; a < expressionList.Count; a++)
                            row.CreateCell(a).SetCellObjectValue(expressionList[a].GetValue(i));
                        r++;
                    }
                    workbook.Write(ms);
                    excelData = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logs.WriteException(ex);
                System.Windows.MessageBox.Show("An error occured while processing the file.", "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            SaveExcelData(excelData, "Save Bills Payment List", string.Format("bills payment_{0:yyyy-MM-dd}", DateTime.Now));
        }

        private static void SaveExcelData(byte[] excelData, string title, string fileName)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Title = title;
            save.Filter = "Excel File|*.xlsx";
            save.FileName = fileName;
            if (save.ShowDialog() == true)
            {
                try
                {
                    using (var fs = new FileStream(save.FileName, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(excelData, 0, excelData.Length);
                    }
                    FileHelper.Open(save.FileName);
                }
                catch (IOException ioEx)
                {
                    Logs.WriteException(ioEx);
                    System.Windows.MessageBox.Show(ioEx.Message);
                }
            }
        }

        private class FieldExpressionCollection<T> : List<FieldExpression<T>>
        {
            public void Add(Expression<Func<T, object>> expression)
            {
                this.Add(expression, null, null, null);
            }

            public void Add(Expression<Func<T, object>> expression, string fieldName)
            {
                this.Add(expression, null, fieldName, null);
            }

            public void Add(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName)
            {
                this.Add(expression, predicate, fieldName, null);
            }

            public void Add(Expression<Func<T, object>> expression, string fieldName, string formatString)
            {
                this.Add(expression, null, fieldName, formatString);
            }

            public void Add(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName, string formatString)
            {
                this.Add(new FieldExpression<T>(expression, predicate, fieldName, formatString));
            }
        }

        private class FieldExpression<T>
        {
            private Expression<Func<T, object>> _expression;
            private string _fieldName;
            private string _formatString;
            private Func<T, bool> _predicate;

            public FieldExpression(Expression<Func<T, object>> expression, Func<T, bool> predicate, string fieldName, string formatString)
            {
                this._expression = expression;
                this._predicate = predicate;
                this._fieldName = fieldName;
                this._formatString = formatString;
            }

            public string GetFieldName()
            {
                if (!string.IsNullOrEmpty(_fieldName))
                    return _fieldName;
                else
                {
                    if (_expression.Body is MemberExpression)
                    {
                        return ((MemberExpression)_expression.Body).Member.Name;
                    }
                    else
                    {
                        var op = ((UnaryExpression)_expression.Body).Operand;
                        return ((MemberExpression)op).Member.Name;
                    }
                }
            }

            public object GetValue(T parent)
            {
                try
                {
                    //check for condition
                    if (_predicate != null && !_predicate(parent))
                        return null;
                }
                catch (NullReferenceException)
                { }

                object value = null;
                try
                {
                    //get value from expression
                    var method = _expression.Compile();
                    value = method(parent);
                }
                catch (NullReferenceException)
                { }
                if (value == null) return null;
                Type type = value.GetType();
                if (Nullable.GetUnderlyingType(type) != null)
                    type = Nullable.GetUnderlyingType(type);
                if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(decimal))
                {
                    //if value is number value, do nothing, leave as is
                }
                else
                {
                    value = GetFormattedStringValue(value);
                }
                return value;
            }

            public string GetStringValue(T parent)
            {
                try
                {
                    //check for condition
                    if (_predicate != null && !_predicate(parent))
                        return null;
                }
                catch (NullReferenceException)
                { }

                object value = null;
                try
                {
                    //get value from expression
                    var method = _expression.Compile();
                    value = method(parent);
                }
                catch (NullReferenceException)
                { }

                if (value != null)
                {
                    value = GetFormattedStringValue(value);
                }
                return Convert.ToString(value);
            }

            private string GetFormattedStringValue(object value)
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    string format = !string.IsNullOrEmpty(this._formatString) ? this._formatString : GetFormatStringForType(value.GetType());
                    return formattable.ToString(format, CultureInfo.CurrentCulture);
                }
                return Convert.ToString(value);
            }

            private static string GetFormatStringForType(Type type)
            {
                if (type != null)
                {
                    if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                        return "#.#0";
                    else if (type == typeof(DateTime))
                        return "M/d/yyyy";
                    else if (type == typeof(TimeSpan))
                        return "c";
                }
                return "";
            }
        }
    }
}
