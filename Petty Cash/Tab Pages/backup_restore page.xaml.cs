using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using Petty_Cash.Classes;

namespace Petty_Cash.Tab_Pages
{
    /// <summary>
    /// Interaction logic for backup_restore_page.xaml
    /// </summary>
    public partial class backup_restore_page : Page
    {
        public class BackupInfo
        {
            public string File { get; set; }
            public string Name { get; set; }
            public DateTime Time { get; set; }

            public BackupInfo(string file)
            {
                this.File = file;
                this.Name = Path.GetFileName(file);
                this.Time = System.IO.File.GetLastWriteTime(file);
            }
        }

        private ObservableCollection<BackupInfo> backupFilesList;

        public backup_restore_page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            backupFilesList = new ObservableCollection<BackupInfo>(BackupRestoreHelper.GetBackupFiles().Select(f => new BackupInfo(f)).OrderByDescending(i => i.Time));
            restoreDataGrid.ItemsSource = backupFilesList;
        }

        private void Backup_btn_Click(object sender, RoutedEventArgs e)
        {
            BackupRestoreHelper.DoBackup();
            LoadData();
        }

        private void Restore_btn_Click(object sender, RoutedEventArgs e)
        {
            BackupInfo backup = restoreDataGrid.SelectedItem as BackupInfo;
            if (backup != null)
                BackupRestoreHelper.DoRestore(backup.File);

        }
    }
}
