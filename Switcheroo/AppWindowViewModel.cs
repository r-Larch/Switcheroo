﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Switcheroo.Core;


namespace Switcheroo {
    public class AppWindowViewModel : INotifyPropertyChanged, IWindowText {
        public AppWindowViewModel(AppWindow appWindow)
        {
            AppWindow = appWindow;
        }

        public AppWindow AppWindow { get; }

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
