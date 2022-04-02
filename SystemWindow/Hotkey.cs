/*
 * ManagedWinapi - A collection of .NET components that wrap PInvoke calls to
 * access native API by managed code. http://mwinapi.sourceforge.net/
 * Copyright (C) 2006 Michael Schierl
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; see the file COPYING. if not, visit
 * http://www.gnu.org/licenses/lgpl.html or write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System.Runtime.InteropServices;
using ManagedWinapi.Windows;


namespace ManagedWinapi {
    /// <summary>
    ///     Specifies a component that creates a global keyboard hotkey.
    /// </summary>
    public class Hotkey : IDisposable {
        private static int _hotkeyCounter = 0xA000;
        private readonly IntPtr _hWnd;
        private bool _ctrl, _alt, _shift, _windows;
        private int _virtualKeyCode;

        private readonly int _hotkeyIndex;
        private bool _isDisposed, _isEnabled, _isRegistered;


        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public Hotkey()
        {
            EventDispatchingNativeWindow.Instance.EventHandler += nw_EventHandler;
            _hotkeyIndex = Interlocked.Increment(ref _hotkeyCounter);

            _hWnd = EventDispatchingNativeWindow.Instance.Handle;
        }

        /// <summary>
        ///     Enables the hotkey. When the hotkey is enabled, pressing it causes a
        ///     <c>HotkeyPressed</c> event instead of being handled by the active
        ///     application.
        /// </summary>
        public bool Enabled {
            get => _isEnabled;
            set {
                _isEnabled = value;
                UpdateHotkey(false);
            }
        }

        /// <summary>
        ///     The key code of the hotkey.
        /// </summary>
        public int VirtualKeyCode {
            get => _virtualKeyCode;

            set {
                _virtualKeyCode = value;
                UpdateHotkey(true);
            }
        }

        /// <summary>
        ///     Whether the shortcut includes the Control modifier.
        /// </summary>
        public bool Ctrl {
            get => _ctrl;
            set {
                _ctrl = value;
                UpdateHotkey(true);
            }
        }

        /// <summary>
        ///     Whether this shortcut includes the Alt modifier.
        /// </summary>
        public bool Alt {
            get => _alt;
            set {
                _alt = value;
                UpdateHotkey(true);
            }
        }

        /// <summary>
        ///     Whether this shortcut includes the shift modifier.
        /// </summary>
        public bool Shift {
            get => _shift;
            set {
                _shift = value;
                UpdateHotkey(true);
            }
        }

        /// <summary>
        ///     Whether this shortcut includes the Windows key modifier. The windows key
        ///     is an addition by Microsoft to the keyboard layout. It is located between
        ///     Control and Alt and depicts a Windows flag.
        /// </summary>
        public bool WindowsKey {
            get => _windows;
            set {
                _windows = value;
                UpdateHotkey(true);
            }
        }

        /// <summary>
        ///     Occurs when the hotkey is pressed.
        /// </summary>
        public event EventHandler? HotkeyPressed;

        private void nw_EventHandler(ref Message m, ref bool handled)
        {
            if (handled) return;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == _hotkeyIndex) {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
        }


        private void UpdateHotkey(bool reregister)
        {
            var shouldBeRegistered = _isEnabled && !_isDisposed;
            if (_isRegistered && (!shouldBeRegistered || reregister)) {
                // unregister hotkey
                UnregisterHotKey(_hWnd, id: _hotkeyIndex);
                _isRegistered = false;
            }

            if (!_isRegistered && shouldBeRegistered) {
                // register hotkey
                var success = RegisterHotKey(_hWnd, id: _hotkeyIndex,
                    fsModifiers: (_shift ? MOD_SHIFT : 0) + (_ctrl ? MOD_CONTROL : 0) + (_alt ? MOD_ALT : 0) + (_windows ? MOD_WIN : 0),
                    vlc: (int) _virtualKeyCode
                );
                if (!success) throw new HotkeyAlreadyInUseException();
                _isRegistered = true;
            }
        }


        /// <summary>
        ///     Releases all resources used by the System.ComponentModel.Component.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;
            UpdateHotkey(false);
            EventDispatchingNativeWindow.Instance.EventHandler -= nw_EventHandler;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Hotkey() => Dispose(false);


        #region PInvoke Declarations

        // ReSharper disable InconsistentNaming

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int MOD_ALT = 0x0001;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int MOD_WIN = 0x0008;

        private const int WM_HOTKEY = 0x0312;

        #endregion
    }

    /// <summary>
    ///     The exception is thrown when a hotkey should be registered that
    ///     has already been registered by another application.
    /// </summary>
    public class HotkeyAlreadyInUseException : Exception {
    }
}
