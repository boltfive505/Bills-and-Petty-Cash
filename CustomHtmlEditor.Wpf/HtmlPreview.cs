using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CustomHtmlEditor.Wpf
{
    [TemplatePart(Name = "PART_WebBrowser", Type = typeof(WebBrowser))]
    public class HtmlPreview : Control
    {
        private const string ELEMENT_WEBBROWSER = "PART_WebBrowser";
        private WebBrowser _webBrowser;

        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(nameof(HtmlContent), typeof(string), typeof(HtmlPreview), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHtmlContentPropertyChanged)));
        public string HtmlContent
        {
            get { return (string)GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public bool IsContentEmpty
        {
            get { return IsEditorAvailable() ? Convert.ToBoolean(InvokeScript("is_content_empty")) : true; }
        }

        static HtmlPreview()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlPreview), new FrameworkPropertyMetadata(typeof(HtmlPreview)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _webBrowser = this.GetTemplateChild(ELEMENT_WEBBROWSER) as WebBrowser;
            string htmlFile = HtmlHelpers.ExtractSimplePreviewFiles();
            LoadHtmlFile(htmlFile);
        }

        protected virtual void LoadHtmlFile(string htmlFile)
        {
            _webBrowser.LoadCompleted += _webBrowser_LoadCompleted;
            _webBrowser.Navigate(htmlFile);
        }

        private void _webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SetText(HtmlContent);
            _webBrowser.LoadCompleted -= _webBrowser_LoadCompleted;
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlPreview)
            {
                HtmlPreview obj = sender as HtmlPreview;
                if (obj.IsEditorAvailable())
                {
                    obj.SetText((string)e.NewValue);
                }
            }
        }

        protected virtual bool IsEditorAvailable()
        {
            return _webBrowser?.Document != null;
        }

        protected virtual object InvokeScript(string methodName, params object[] args)
        {
            return _webBrowser.InvokeScript(methodName, args);
        }

        private string GetText()
        {
            if (!IsEditorAvailable()) return string.Empty;
            return Convert.ToString(InvokeScript("get_content"));
        }

        private void SetText(string html)
        {
            if (!IsEditorAvailable()) return;
            if (html == null) html = "";
            InvokeScript("set_content", html);
        }
    }
}
