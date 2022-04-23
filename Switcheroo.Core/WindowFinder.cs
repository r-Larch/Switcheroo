/*
 * Switcheroo - The incremental-search task switcher for Windows.
 * http://www.switcheroo.io/
 * Copyright 2009, 2010 James Sulak
 * Copyright 2014 Regin Larsen
 *
 * Switcheroo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Switcheroo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Switcheroo.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using ManagedWinapi.Windows;
using frigo = FrigoTab;


namespace Switcheroo.Core {
    public class WindowFinder {
        private SystemWindow _foregroundWindow;

        public bool IsWindowsNativeTaskSwitcherActive => _foregroundWindow.ClassName == "MultitaskingViewFrame";

        public List<AppWindow> GetWindows()
        {
            var appWindows = new frigo.WindowFinder().Windows.ToList();
            var filtered = AppWindow.AllTopLevelWindows
                .Where(a => {
                    var match = appWindows.Find(h => h.Handle == a.HWnd) != frigo.WindowHandle.Null;
                    return a.IsAltTabWindow() && match;
                }).ToList();

            foreach (var appWindow in filtered) {
                appWindow.IsForegroundWindow = IsForegroundWindow(appWindow);
            }

            return filtered;
        }

        public void SaveForegroundWindow()
        {
            _foregroundWindow = SystemWindow.ForegroundWindow;
        }

        private bool IsForegroundWindow(SystemWindow appWindow)
        {
            return _foregroundWindow.HWnd == appWindow.HWnd || _foregroundWindow.Process.Id == appWindow.Process.Id;
        }
    }
}
