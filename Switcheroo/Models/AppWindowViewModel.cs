using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Switcheroo.Core;
using Switcheroo.Core.Matchers;


namespace Switcheroo {
    public class AppWindowViewModel : INotifyPropertyChanged, IWindowText {
        public AppWindowViewModel(AppWindow appWindow)
        {
            AppWindow = appWindow;
            FormattedTitle = new XamlHighlighter().Highlight(new[] { new StringPart(WindowTitle) });
            FormattedProcessTitle = new XamlHighlighter().Highlight(new[] { new StringPart(ProcessTitle) });
        }

        private AppWindow AppWindow { get; }


        public bool IsForegroundWindow => AppWindow.IsForegroundWindow;


        #region Actions

        public void SwitchToThisWindow() => AppWindow.SwitchToLastVisibleActivePopup();

        public void RunDuplicateProcess()
        {
            try {
                var cmd = AppWindow.ExecutablePath;
                Process.Start(cmd);
            }
            catch (Win32Exception ex) {
                // Win32Exception (5): An error occurred trying to start process 'C:\Program Files\WindowsApps\...exe' with working directory '..'. Access is denied.
                Console.WriteLine(ex);
                throw;
            }
        }

        public async Task<bool> TryCloseAsync(CancellationToken token)
        {
            IsBeingClosed = true;
            AppWindow.Close();

            var checkInterval = TimeSpan.FromMilliseconds(125);

            while (!token.IsCancellationRequested && !AppWindow.IsClosedOrHidden) {
                await Task.Delay(checkInterval, token).ConfigureAwait(false);
            }

            return AppWindow.IsClosedOrHidden;
        }

        #endregion


        #region IWindowText Members

        public string WindowTitle => AppWindow.Title;

        public string ProcessTitle => AppWindow.ProcessTitle;

        #endregion


        #region Bindable properties

        public IntPtr HWnd => AppWindow.HWnd;

        private string _formattedTitle;

        public string FormattedTitle {
            get => _formattedTitle;
            set {
                _formattedTitle = value;
                NotifyOfPropertyChange(() => FormattedTitle);
            }
        }

        private string _formattedProcessTitle;

        public string FormattedProcessTitle {
            get => _formattedProcessTitle;
            set {
                _formattedProcessTitle = value;
                NotifyOfPropertyChange(() => FormattedProcessTitle);
            }
        }

        private bool _isBeingClosed;

        public bool IsBeingClosed {
            get => _isBeingClosed;
            set {
                _isBeingClosed = value;
                NotifyOfPropertyChange(() => IsBeingClosed);
            }
        }

        #endregion


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyOfPropertyChange<T>(Expression<Func<T>> property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(GetPropertyName(property)));
        }

        private string GetPropertyName<T>(Expression<Func<T>> property)
        {
            var lambda = (LambdaExpression) property;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression) {
                var unaryExpression = (UnaryExpression) lambda.Body;
                memberExpression = (MemberExpression) unaryExpression.Operand;
            }
            else {
                memberExpression = (MemberExpression) lambda.Body;
            }

            return memberExpression.Member.Name;
        }

        #endregion
    }
}
