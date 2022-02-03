using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using bolt5.ModalWpf;

namespace Petty_Cash.Sub_Modals
{
    /// <summary>
    /// Interaction logic for payeeAddEdit_modal.xaml
    /// </summary>
    public partial class payeeAddEdit_modal : UserControl, IModalClosing
    {
        public payeeAddEdit_modal()
        {
            InitializeComponent();
        }

        public void ModalClosing(ModalClosingArgs e)
        {
            if (e.Result == ModalResult.Save)
            {
                //if setting to InActive, check if balance is zero
                if (!(IsActiveValue.IsChecked ?? false))
                {
                    decimal balance = (this.DataContext as Objects.PayeeViewModel).Amount;
                    if (balance != 0)
                    {
                        MessageBox.Show("Cannon set Payee to InActive.\n\nReason: Payee still has Balance Amount.", "Edit Payee", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Cancel = true;
                    }
                }
            }
        }

        private void IsActiveValue_Unchecked(object sender, RoutedEventArgs e)
        {
            //if setting to InActive (unchecked), check if balance is zero
            decimal balance = (this.DataContext as Objects.PayeeViewModel).Amount;
            if (balance != 0)
            {
                //dont allow set to InActive if there is still balance
                (sender as CheckBox).IsChecked = true;
                MessageBox.Show("Cannon set Payee to InActive.\n\nReason: Payee still has Balance Amount.", "Edit Payee", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
