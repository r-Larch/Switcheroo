using System;
using System.Windows.Media;
using Switcheroo.Properties;


namespace Switcheroo {
    public static class Theme {
        private static SolidColorBrush _background;
        private static SolidColorBrush _foreground;
        private static MainWindow _mainWindow;

        public static void SubscribeWindow(MainWindow main)
        {
            _mainWindow = main;
        }

        public static void LoadTheme()
        {
            Enum.TryParse(Settings.Default.Theme, out Mode mode);
            switch (mode) {
                case Mode.Light:
                    _background = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                    _foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    break;
                case Mode.Dark:
                    _background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    _foreground = new SolidColorBrush(Color.FromRgb(248, 248, 248));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            SetUpTheme();
        }

        private static void SetUpTheme()
        {
            _mainWindow.Border.Background =
                _mainWindow.tb.Background =
                    _mainWindow.lb.Background =
                        _mainWindow.Border.BorderBrush =
                            _background;

            _mainWindow.tb.Foreground =
                _mainWindow.lb.Foreground =
                    _foreground;
        }

        private enum Mode {
            Light,
            Dark
        }
    }
}
