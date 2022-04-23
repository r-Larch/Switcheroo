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

using ManagedWinapi;
using Switcheroo.Properties;


namespace Switcheroo {
    public class HotKey : Hotkey {
        public void LoadSettings()
        {
            VirtualKeyCode = Settings.Default.HotKey;
            WindowsKey = Settings.Default.WindowsKey;
            Alt = Settings.Default.Alt;
            Ctrl = Settings.Default.Ctrl;
            Shift = Settings.Default.Shift;
        }

        public void SaveSettings()
        {
            Settings.Default.HotKey = VirtualKeyCode;
            Settings.Default.WindowsKey = WindowsKey;
            Settings.Default.Alt = Alt;
            Settings.Default.Ctrl = Ctrl;
            Settings.Default.Shift = Shift;
            Settings.Default.Save();
        }
    }
}
