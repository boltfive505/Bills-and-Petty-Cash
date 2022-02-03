using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Petty_Cash.Model;
using Petty_Cash.Bills.Model;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Petty_Cash
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (InstanceManager.IsInstanceAlreadyRunning())
            {
                Application.Current.Shutdown();
                return;
            }
            InitializeErrorHandling();
            InitializeDatabase();

            Window main = new MainWindow();
            main.Show();
        }

        private void InitializeDatabase()
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(InitializeDatabaseTask<PettyCashModel>(ctx => ctx.Initialize()));
            tasks.Add(InitializeDatabaseTask<BillsModel>(ctx => ctx.Initialize()));
            tasks.Add(InitializeDatabaseTask<Tutorials.Model.TutorialsModel>(ctx => ctx.Initialize()));
            
            CancellationTokenSource cancel = new CancellationTokenSource();
            try
            {
                Task.WaitAll(tasks.ToArray(), cancel.Token);
            }
            catch (Exception ex)
            {
                cancel.Cancel();
                throw ex;
            }
        }

        private static Task InitializeDatabaseTask<T>(Action<T> initialize) where T : System.Data.Entity.DbContext, new()
        {
            return Task.Run(() =>
            {
                using (var context = new T())
                {
                    initialize?.Invoke(context);
                }
            });
        }

        private void InitializeErrorHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            Logs.WriteException(ex);
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            Logs.WriteException(ex);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            Logs.WriteException(ex);
        }
    }
}
