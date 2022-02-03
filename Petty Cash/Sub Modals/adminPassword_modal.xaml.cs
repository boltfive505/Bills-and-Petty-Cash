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
    /// Interaction logic for adminPassword_modal.xaml
    /// </summary>
    public partial class adminPassword_modal : UserControl, IModalClosing
    {
        public adminPassword_modal()
        {
            InitializeComponent();
        }

        public void ModalClosing(ModalClosingArgs e)
        {            
            if (e.Result == ModalResult.Yes)
            {
                string adminPassword = Classes.MiscHelper.GetAdminPassword(); //get admin password
                string enteredPassword = passwordTxt.Password;
                if (adminPassword != enteredPassword)
                    e.Cancel = true;
            }
        }
    }
}
