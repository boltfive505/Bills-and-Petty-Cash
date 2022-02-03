using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;

namespace Petty_Cash.Reports
{
    public partial class report_form : Form
    {
        private ReportViewer reportViewer;

        public static event Action IsPrinted;

        public report_form(string reportFileName)
        {
            InitializeComponent();

            reportViewer = new ReportViewer();
            reportViewer.ReportError += ReportViewer_ReportError;
            reportViewer.PrintingBegin += ReportViewer_PrintingBegin;
            this.Controls.Add(reportViewer);

            reportViewer.Dock = DockStyle.Fill;
            reportViewer.PageCountMode = PageCountMode.Actual;
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer.ZoomMode = ZoomMode.PageWidth;
            reportViewer.ShowBackButton = false;
            reportViewer.ShowCredentialPrompts = false;
            reportViewer.ShowDocumentMapButton = false;
            reportViewer.ShowFindControls = false;
            reportViewer.ShowRefreshButton = false;
            reportViewer.ShowStopButton = false;

            reportViewer.LocalReport.EnableExternalImages = true;
            reportViewer.LocalReport.EnableHyperlinks = true;
            reportViewer.LocalReport.ReportEmbeddedResource = "Petty_Cash.Reports.ReportFiles." + reportFileName + ".rdlc";
            reportViewer.LocalReport.DataSources.Clear();
        }

        private void ReportViewer_PrintingBegin(object sender, ReportPrintEventArgs e)
        {
            report_form.IsPrinted?.Invoke();
        }

        private void ReportViewer_ReportError(object sender, ReportErrorEventArgs e)
        {
            Logs.WriteException(e.Exception);
        }

        private void report_form_Load(object sender, EventArgs e)
        {
            reportViewer.RefreshReport();
        }

        public void AddDataSet(string datasourceName, object datasource)
        {
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource(datasourceName, datasource));
        }

        public static void ShowReport(string reportFileName, string datasourceName, object datasource)
        {
            report_form.ShowReport(reportFileName, new Dictionary<string, object>() { { datasourceName, datasource } });
        }

        public static void ShowReport(string reportFileName, Dictionary<string, object> datasourceCollection)
        {
            var frm = new report_form(reportFileName);
            foreach (var i in datasourceCollection)
                frm.AddDataSet(i.Key, i.Value);
            frm.ShowDialog();
        }
    }
}
