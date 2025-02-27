﻿using Windows.Win32;


namespace Switcheroo.Windows;

// Convert a keycode to the relevant display character
// http://stackoverflow.com/a/375047/198065
public struct KeyboardHelper {
    public static unsafe string CodeToString(uint virtualKey)
    {
        var thread = PInvoke.GetWindowThreadProcessId(0);
        var hkl = PInvoke.GetKeyboardLayout(thread);
        if (hkl == IntPtr.Zero) {
            return string.Empty;
        }

        var scanCode = PInvoke.MapVirtualKeyEx(virtualKey, (uint) MapVirtualKeyMapTypes.MAPVK_VK_TO_CHAR, hkl);

        var keyStates = new byte[256];
        if (!PInvoke.GetKeyboardState(keyStates)) {
            return string.Empty;
        }

        const int size = 10;
        fixed (char* sb = new char[10]) {
            var rc = PInvoke.ToUnicodeEx(virtualKey, scanCode, keyStates, sb, size, 0, hkl);
            return new string(sb);
        }
    }

    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo
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
}
