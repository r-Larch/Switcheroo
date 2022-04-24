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

using System;
using System.Runtime.InteropServices;
using System.Text;


namespace Switcheroo.Core;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
internal static class WinApi {
    /// <summary>
    ///     The set of valid MapTypes used in MapVirtualKey
    /// </summary>
    public enum MapVirtualKeyMapTypes : uint {
        /// <summary>
        ///     uCode is a virtual-key code and is translated into a scan code.
        ///     If it is a virtual-key code that does not distinguish between left- and
        ///     right-hand keys, the left-hand scan code is returned.
        ///     If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_VSC = 0x00,

        /// <summary>
        ///     uCode is a scan code and is translated into a virtual-key code that
        ///     does not distinguish between left- and right-hand keys. If there is no
        ///     translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK = 0x01,

        /// <summary>
        ///     uCode is a virtual-key code and is translated into an unshifted
        ///     character value in the low-order word of the return value. Dead keys (diacritics)
        ///     are indicated by setting the top bit of the return value. If there is no
        ///     translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_CHAR = 0x02,

        /// <summary>
        ///     Windows NT/2000/XP: uCode is a scan code and is translated into a
        ///     virtual-key code that distinguishes between left- and right-hand keys. If
        ///     there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK_EX = 0x03,

        /// <summary>
        ///     Not currently documented
        /// </summary>
        MAPVK_VK_TO_VSC_EX = 0x04
    }


    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr GetKeyboardLayout(uint threadId);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool GetKeyboardState(byte[] keyStates);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern uint GetWindowThreadProcessId(IntPtr hwindow, out uint processId);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKeyEx(uint uCode, MapVirtualKeyMapTypes uMapType, IntPtr dwhkl);
}
