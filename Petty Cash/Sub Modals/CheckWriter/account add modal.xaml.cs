using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Petty_Cash.Classes;
using Petty_Cash.Objects.CheckWriter;

namespace Petty_Cash.Sub_Modals.CheckWriter
{
    /// <summary>
    /// Interaction logic for account_add_modal.xaml
    /// </summary>
    public partial class account_add_modal : UserControl
    {
        public account_add_modal()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var bankTask = CheckWriterHelper.GetBankListAsync();
            bankTask.Wait();
            BankValue.ItemsSource = new ObservableCollection<CheckWriterBankViewModel>(bankTask.Result.OrderBy(i => i.BankName));
        }
    }
}
