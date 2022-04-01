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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ManagedWinapi.Windows;


namespace ManagedWinapi.Audio.Mixer {
    /// <summary>
    ///     Represents a mixer provided by a sound card. Each mixer has
    ///     multiple destination lines (e. g. Record and Playback) of which
    ///     each has multiple source lines (Wave, MIDI, Mic, etc.).
    /// </summary>
    public class Mixer : IDisposable {
        /// <summary>
        ///     Occurs when a control of this mixer changes value.
        /// </summary>
        public MixerEventHandler ControlChanged;
        private IList<DestinationLine> destLines;

        private IntPtr hMixer;

        /// <summary>
        ///     Occurs when a line of this mixer changes.
        /// </summary>
        public MixerEventHandler LineChanged;
        private readonly MIXERCAPS mc;

        private Mixer(IntPtr hMixer)
        {
            this.hMixer = hMixer;
            EventDispatchingNativeWindow.Instance.EventHandler += ednw_EventHandler;
            mixerGetDevCapsA(hMixer, ref mc, Marshal.SizeOf(mc));
        }

        /// <summary>
        ///     Gets the number of available mixers in this system.
        /// </summary>
        public static uint MixerCount => mixerGetNumDevs();

        /// <summary>
        ///     Whether to create change events.
        ///     Enabling this may create a slight performance impact, so only
        ///     enable it if you handle these events.
        /// </summary>
        public bool CreateEvents { get; set; }

        internal IntPtr Handle => hMixer;

        /// <summary>
        ///     Gets the name of this mixer's sound card.
        /// </summary>
        public string Name => mc.szPname;

        /// <summary>
        ///     Gets the number of destination lines of this mixer.
        /// </summary>
        public int DestinationLineCount => mc.cDestinations;

        /// <summary>
        ///     Gets all destination lines of this mixer
        /// </summary>
        public IList<DestinationLine> DestinationLines {
            get {
                if (destLines == null) {
                    var dlc = DestinationLineCount;
                    var l = new List<DestinationLine>(dlc);
                    for (var i = 0; i < dlc; i++) {
                        l.Add(DestinationLine.GetLine(this, i));
                    }

                    destLines = l.AsReadOnly();
                }

                return destLines;
            }
        }

        /// <summary>
        ///     Disposes this mixer.
        /// </summary>
        public void Dispose()
        {
            if (destLines != null) {
                foreach (var dl in destLines) {
                    dl.Dispose();
                }

                destLines = null;
            }

            if (hMixer.ToInt32() != 0) {
                mixerClose(hMixer);
                EventDispatchingNativeWindow.Instance.EventHandler -= ednw_EventHandler;
                hMixer = IntPtr.Zero;
            }
        }

        /// <summary>
        ///     Opens a mixer.
        /// </summary>
        /// <param name="index">The zero-based index of this mixer.</param>
        /// <returns>A reference to this mixer.</returns>
        public static Mixer OpenMixer(uint index)
        {
            if (index < 0 || index > MixerCount)
                throw new ArgumentException();
            var hMixer = IntPtr.Zero;
            var ednw = EventDispatchingNativeWindow.Instance;
            var error = mixerOpen(ref hMixer, index, ednw.Handle, IntPtr.Zero, CALLBACK_WINDOW);
            if (error != 0) {
                throw new Win32Exception("Could not load mixer: " + error);
            }

            return new Mixer(hMixer);
        }

        private void ednw_EventHandler(ref Message m, ref bool handled)
        {
            if (!CreateEvents) return;
            if (m.Msg == MM_MIXM_CONTROL_CHANGE && m.WParam == hMixer) {
                var ctrlID = m.LParam.ToInt32();
                var c = FindControl(ctrlID);
                if (c != null) {
                    if (ControlChanged != null) {
                        ControlChanged(this, new MixerEventArgs(this, c.Line, c));
                    }

                    c.OnChanged();
                }
            }
            else if (m.Msg == MM_MIXM_LINE_CHANGE && m.WParam == hMixer) {
                var lineID = m.LParam.ToInt32();
                var l = FindLine(lineID);
                if (l != null) {
                    if (ControlChanged != null) {
                        LineChanged(this, new MixerEventArgs(this, l, null));
                    }

                    l.OnChanged();
                }
            }
        }

        /// <summary>
        ///     Find a line of this mixer by ID.
        /// </summary>
        /// <param name="lineId">ID of the line to find</param>
        /// <returns>The line, or <code>null</code> if no line was found.</returns>
        public MixerLine FindLine(int lineId)
        {
            foreach (var dl in DestinationLines) {
                var found = dl.findLine(lineId);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        ///     Find a control of this mixer by ID.
        /// </summary>
        /// <param name="ctrlId">ID of the control to find.</param>
        /// <returns>The control, or <code>null</code> if no control was found.</returns>
        public MixerControl FindControl(int ctrlId)
        {
            foreach (var dl in DestinationLines) {
                var found = dl.findControl(ctrlId);
                if (found != null) return found;
            }

            return null;
        }

        #region PInvoke Declarations

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern uint mixerGetNumDevs();

        [DllImport("winmm.dll")]
        private static extern int mixerOpen(ref IntPtr phmx, uint pMxId,
            IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

        [DllImport("winmm.dll")]
        private static extern int mixerClose(IntPtr hmx);

        [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
        private static extern int mixerGetDevCapsA(IntPtr uMxId, ref MIXERCAPS
            pmxcaps, int cbmxcaps);

        private struct MIXERCAPS {
            public short wMid;
            public short wPid;
            public int vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;
            public int fdwSupport;
            public int cDestinations;
        }

        private static readonly uint CALLBACK_WINDOW = 0x00010000;
        private static readonly int MM_MIXM_LINE_CHANGE = 0x3D0;
        private static readonly int MM_MIXM_CONTROL_CHANGE = 0x3D1;

        #endregion
    }

    /// <summary>
    ///     Represents the method that will handle the <b>LineChanged</b> or
    ///     <b>ControlChanged</b> event of a <see cref="Mixer">Mixer</see>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///     A <see cref="MixerEventArgs">MixerEventArgs</see>
    ///     that contains the event data.
    /// </param>
    public delegate void MixerEventHandler(object sender, MixerEventArgs e);

    /// <summary>
    ///     Provides data for the LineChanged and ControlChanged events of a
    ///     <see cref="Mixer">Mixer</see>.
    /// </summary>
    public class MixerEventArgs : EventArgs {
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="MixerEventArgs">MixerEventArgs</see> class.
        /// </summary>
        /// <param name="mixer">The affected mixer</param>
        /// <param name="line">The affected line</param>
        /// <param name="control">
        ///     The affected control, or <code>null</code>
        ///     if this is a LineChanged event.
        /// </param>
        public MixerEventArgs(Mixer mixer, MixerLine line, MixerControl control)
        {
            Mixer = mixer;
            Line = line;
            Control = control;
        }

        /// <summary>
        ///     The affected mixer.
        /// </summary>
        public Mixer Mixer { get; }

        /// <summary>
        ///     The affected line.
        /// </summary>
        public MixerLine Line { get; }

        /// <summary>
        ///     The affected control.
        /// </summary>
        public MixerControl Control { get; }
    }
}
