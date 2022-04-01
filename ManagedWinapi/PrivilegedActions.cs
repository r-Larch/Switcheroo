using System;
using System.Runtime.InteropServices;


namespace ManagedWinapi {
    /// <summary>
    ///     Collection of miscellaneous actions that cannot be performed as
    ///     a non-administrative user, like shutdown or setting the system time.
    /// </summary>
    public static class PrivilegedActions {
        /// <summary>
        ///     Actions that can be performed at shutdown.
        /// </summary>
        public enum ShutdownAction : uint {
            /// <summary>
            ///     Log off the currently logged-on user.
            /// </summary>
            LogOff = 0x00,

            /// <summary>
            ///     Shut down the system.
            /// </summary>
            ShutDown = 0x01,

            /// <summary>
            ///     Reboot the system.
            /// </summary>
            Reboot = 0x02,

            /// <summary>
            ///     Shut down the system and power it off.
            /// </summary>
            PowerOff = 0x08,

            /// <summary>
            ///     Reboot the system and restart applications that are running
            ///     now and support this feature.
            /// </summary>
            RestartApps = 0x40,
        }

        /// <summary>
        ///     Whether shutdown should be forced if an application cancels it
        ///     or is hung.
        /// </summary>
        public enum ShutdownForceMode : uint {
            /// <summary>
            ///     Do not force shutdown, applications can cancel it.
            /// </summary>
            NoForce = 0x00,

            /// <summary>
            ///     Force shutdown, even if application cancels it or is hung.
            /// </summary>
            Force = 0x04,

            /// <summary>
            ///     Force shutdown if application is hung, but not if it cancels it.
            /// </summary>
            ForceIfHung = 0x10
        }

        /// <summary>
        ///     Get or set the system time in the local timezone.
        /// </summary>
        public static DateTime LocalTime {
            get {
                var st = new SYSTEMTIME();
                ApiHelper.FailIfZero(GetLocalTime(ref st));
                return st.ToDateTime();
            }

            set {
                var st = new SYSTEMTIME(value);
                // Set it twice due to possible daylight savings change
                ApiHelper.FailIfZero(SetLocalTime(ref st));
                ApiHelper.FailIfZero(SetLocalTime(ref st));
            }
        }

        /// <summary>
        ///     Get or set the system time, in UTC.
        /// </summary>
        public static DateTime SystemTime {
            get {
                var st = new SYSTEMTIME();
                ApiHelper.FailIfZero(GetSystemTime(ref st));
                return st.ToDateTime();
            }

            set {
                var st = new SYSTEMTIME(value);
                ApiHelper.FailIfZero(SetLocalTime(ref st));
            }
        }

        /// <summary>
        ///     Shutdown the system.
        /// </summary>
        public static void ShutDown(ShutdownAction action)
        {
            ShutDown(action, ShutdownForceMode.NoForce);
        }

        /// <summary>
        ///     Shutdown the system.
        /// </summary>
        public static void ShutDown(ShutdownAction action, ShutdownForceMode forceMode)
        {
            ApiHelper.FailIfZero(ExitWindowsEx((uint) action | (uint) forceMode, SHTDN_REASON_FLAG_PLANNED));
        }

        #region PInvoke Declarations

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int ExitWindowsEx(uint uFlags, uint dwReason);

        private const uint SHTDN_REASON_FLAG_PLANNED = 0x80000000;

        private struct SYSTEMTIME {
            internal readonly ushort wYear;
            internal readonly ushort wMonth;
            internal ushort wDayOfWeek;
            internal readonly ushort wDay;
            internal readonly ushort wHour;
            internal readonly ushort wMinute;
            internal readonly ushort wSecond;
            internal readonly ushort wMilliseconds;

            internal SYSTEMTIME(DateTime time)
            {
                wYear = (ushort) time.Year;
                wMonth = (ushort) time.Month;
                wDayOfWeek = (ushort) time.DayOfWeek;
                wDay = (ushort) time.Day;
                wHour = (ushort) time.Hour;
                wMinute = (ushort) time.Minute;
                wSecond = (ushort) time.Second;
                wMilliseconds = (ushort) time.Millisecond;
            }

            internal DateTime ToDateTime()
            {
                return new DateTime(wYear, wMonth, wDay, wHour, wMinute, wSecond, wMilliseconds);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int SetSystemTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetLocalTime(ref SYSTEMTIME lpSystemTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int SetLocalTime(ref SYSTEMTIME lpSystemTime);

        #endregion
    }
}
