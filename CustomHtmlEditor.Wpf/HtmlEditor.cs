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
    public class HtmlEditor : Control
    {
        private const string ELEMENT_WEBBROWSER = "PART_WebBrowser";
        private WebBrowser _webBrowser;
        private ObjectForScriptingHelper _objectForScripting;
        private bool _isEditingFlag = false;

        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(nameof(HtmlContent), typeof(string), typeof(HtmlEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHtmlContentPropertyChanged)));
        public string HtmlContent
        {
            get { return (string)GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register(nameof(IsPreview), typeof(bool), typeof(HtmlEditor), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsPreviewPropertyChanged)));
        public bool IsPreview
        {
            get { return (bool)GetValue(IsPreviewProperty); }
            set { SetValue(IsPreviewProperty, value); }
        }

        public bool IsContentEmpty
        {
            get { return IsEditorAvailable() ? Convert.ToBoolean(InvokeScript("is_content_empty")) : true; }
        }

        static HtmlEditor()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlEditor), new FrameworkPropertyMetadata(typeof(HtmlEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _webBrowser = this.GetTemplateChild(ELEMENT_WEBBROWSER) as WebBrowser;
            string htmlFile = HtmlHelpers.ExtractWysiwygEditorFiles();
            LoadHtmlFile(htmlFile);
        }

        protected virtual void LoadHtmlFile(string htmlFile)
        {
            _webBrowser.LoadCompleted += _webBrowser_LoadCompleted;
            _webBrowser.Navigate(htmlFile);
        }

        private void _webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            _objectForScripting = new ObjectForScriptingHelper();
            SetText(HtmlContent);
            SetPreview(IsPreview);
            _objectForScripting.ContentChange += _objectForScripting_ContentChange;
            _webBrowser.ObjectForScripting = _objectForScripting;
            _webBrowser.LoadCompleted -= _webBrowser_LoadCompleted;
        }

        private void _objectForScripting_ContentChange()
        {
            _isEditingFlag = true;
            string txt = GetText();
            SetValue(HtmlContentProperty, txt);
            _isEditingFlag = false;
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlEditor)
            {
                HtmlEditor obj = sender as HtmlEditor;
                if (!obj._isEditingFlag && obj.IsEditorAvailable())
                {
                    obj.SetText((string)e.NewValue);
                }
            }
        }

        private static void OnIsPreviewPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlEditor)
            {
                HtmlEditor obj = sender as HtmlEditor;
                if (obj.IsEditorAvailable())
                {
                    obj.SetPreview((bool)e.NewValue);
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
            return Convert.ToString(InvokeScript("get_content_code"));
        }

        private void SetText(string html)
        {
            if (!IsEditorAvailable()) return;
            InvokeScript("set_content_code", html);
        }

        private void SetPreview(bool preview)
        {
            if (!IsEditorAvailable()) return;
            InvokeScript("set_enabled", !preview);
            InvokeScript("show_toolbar", !preview);
        }
    }
}
