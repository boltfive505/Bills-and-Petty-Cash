using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Petty_Cash.Classes;
using System.Text.RegularExpressions;
using Xceed.Wpf.Toolkit;
using bolt5.CustomControls;
using bolt5.ModalWpf;
using System.Collections.ObjectModel;
using Petty_Cash.Objects.Bills;
using Petty_Cash.Objects.CheckWriter;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for billPaymentAddEdit_modal.xaml
    /// </summary>
    public partial class billPaymentAddEdit_modal : UserControl, IModalClosed
    {
        public billPaymentAddEdit_modal()
        {
            InitializeComponent();
            BillingPeriodInput.LostFocus += BillingPeriodInput_LostFocus;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                paymentDg.ItemsSource = new ObservableCollection<BillPaymentViewModel>(BillsPeriodContextHelper.GetBillPaymentListAsync((this.DataContext as BillPaymentViewModel).Period.Id).GetResult().OrderByDescending(i => i.PeriodDate));
                BankCheckValue.ItemsSource = new ObservableCollection<CheckWriterCheckViewModel>(CheckWriterHelper.GetCheckListAync(false).GetResult().OrderByDescending(i => i.UpdatedDate));
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void GetOCR_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Convert.ToString((sender as FrameworkElement).Tag);
            string file1 = FileHelper.GetFile(fileName, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY); //look for file in local uploads
            string ocrText = "";
            string fileToOcr = "";
            if (File.Exists(fileName))
                fileToOcr = fileName; //file is newly uploaded
            else if (File.Exists(file1))
                fileToOcr = file1; //file is old
            ocrText = OcrHelper.GetTextFromFile(fileToOcr);
            OcrHelper.OpenTextToNotepad(ocrText);
        }

        private void SetPeriodInput_Click(object sender, RoutedEventArgs e)
        {
            BillingPeriodInput.Text = string.Format("{0:MM/dd/yyyy} - {1:MM/dd/yyyy}", BillingPeriodStart.Value, BillingPeriodEnd.Value);
        }

        private static bool GetBillingPeriodFromInput(string input, out DateTime? start, out DateTime? end)
        {
            input = input.Replace(" ", "");
            var pattern = @"\d{1,4}[\/-]\d{1,4}[\/-]\d{1,4}";
            var matches = Regex.Matches(input, pattern);
            if (matches.Count == 2)
            {
                DateTime value1, value2;
                bool flag1 = DateTime.TryParse(matches[0].Value, out value1);
                bool flag2 = DateTime.TryParse(matches[1].Value, out value2);
                if (flag1 && flag2)
                {
                    start = value1;
                    end = value2;
                    return true;
                }
            }
            start = null;
            end = null;
            return false;
        }

        private void BillingPeriodInput_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime? start, end;
            if (GetBillingPeriodFromInput(BillingPeriodInput.Text, out start, out end))
            {
                BillingPeriodInput.Text = string.Format("{0:MM/dd/yyyy} - {1:MM/dd/yyyy}", start, end);
            }
            else
            {
                BillingPeriodInput.Text = string.Empty;
            }
        }

        public void ModalClosed(ModalClosedArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                DateTime? start, end;
                if (GetBillingPeriodFromInput(BillingPeriodInput.Text, out start, out end))
                {
                    BillingPeriodStart.SetValue(DateTimePicker.ValueProperty, start);
                    BillingPeriodEnd.SetValue(DateTimePicker.ValueProperty, end);
                }
            }
        }

        private void OpenCamera_btn_Click(object sender, RoutedEventArgs e)
        {
            openCamera_modal camera = new openCamera_modal();
            if (ModalForm.ShowModal(camera, "Capture File", ModalButtons.YesNo) == ModalResult.Yes)
            {
                FileAttachment elem = (sender as FrameworkElement).Tag as FileAttachment;
                if (elem != null)
                {
                    elem.SetValue(FileAttachment.FileNameProperty, camera.GetCapturedImageFile());
                }
            }
        }

        private void OpenFile_btn_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (string)(sender as FrameworkElement).Tag;
            string file = FileHelper.GetFile(fileName, BillsPeriodContextHelper.BILLS_FILE_SUBDIRECTORY);
            if (File.Exists(file))
            {
                FileHelper.Open(file);
            }
            else
            {
                System.Windows.MessageBox.Show("ERROR: File not found", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool BankCheckValue_SearchText(object item, string searchText)
        {
            string keyword = searchText.Trim();
            var check = item as CheckWriterCheckViewModel;
            return check.Id.ToString().StartsWith(keyword, StringComparison.InvariantCultureIgnoreCase) ||
                check.CheckNumber.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                check.PayeeName.IndexOf(keyword, 0, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
