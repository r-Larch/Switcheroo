using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Switcheroo.Core;
using Switcheroo.Core.Matchers;
using Switcheroo.Windows;
using WindowFinder = Switcheroo.Windows.WindowFinder;


namespace Switcheroo {
    public class AppWindowViewModel : INotifyPropertyChanged, IWindowText {
        public AppWindowViewModel(AppWindow appAppWindow, WindowFinder windowFinder)
        {
            AppAppWindow = appAppWindow;
            IsForegroundWindow = windowFinder.IsForegroundWindow(appAppWindow);
            FormattedTitle = new XamlHighlighter().Highlight(new[] { new StringPart(WindowTitle) });
            FormattedProcessTitle = new XamlHighlighter().Highlight(new[] { new StringPart(ProcessTitle) });
        }

        private AppWindow AppAppWindow { get; }


        public bool IsForegroundWindow { get; }


        #region Actions

        public void SwitchToThisWindow() => AppAppWindow.SwitchToLastVisibleActivePopup();

        public void RunDuplicateProcess()
        {
            try {
                var cmd = AppAppWindow.ExecutablePath;
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
            AppAppWindow.Close();

            var checkInterval = TimeSpan.FromMilliseconds(125);

            while (!token.IsCancellationRequested && !AppAppWindow.IsClosedOrHidden) {
                await Task.Delay(checkInterval, token).ConfigureAwait(false);
            }

            return AppAppWindow.IsClosedOrHidden;
        }

        #endregion


        #region IWindowText Members

        public string WindowTitle => AppAppWindow.Title;

        public string ProcessTitle {
            get {
                var key = $"ProcessTitle-{AppAppWindow}";
                if (MemoryCache.Default.Get(key) is not string processTitle) {
                    processTitle = AppAppWindow.ProcessTitle;
                    MemoryCache.Default.Add(key, processTitle, DateTimeOffset.Now.AddHours(1));
                }

                return processTitle;
            }
        }

        #endregion


        #region Bindable properties

        public IntPtr HWnd => AppAppWindow.HWnd;

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
