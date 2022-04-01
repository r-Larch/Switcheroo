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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;


namespace ManagedWinapi.Windows {
    /// <summary>
    ///     Any list view, including those from other applications.
    /// </summary>
    public class SystemListView {
        private readonly SystemWindow sw;

        private SystemListView(SystemWindow sw)
        {
            this.sw = sw;
        }

        /// <summary>
        ///     The number of items (icons) in this list view.
        /// </summary>
        public int Count => sw.SendGetMessage(LVM_GETITEMCOUNT);

        /// <summary>
        ///     An item of this list view.
        /// </summary>
        public SystemListViewItem this[int index] => this[index, 0];

        /// <summary>
        ///     A subitem (a column value) of an item of this list view.
        /// </summary>
        public SystemListViewItem this[int index, int subIndex] {
            get {
                var lvi = new LVITEM();
                lvi.cchTextMax = 300;
                lvi.iItem = index;
                lvi.iSubItem = subIndex;
                lvi.stateMask = 0xffffffff;
                lvi.mask = LVIF_IMAGE | LVIF_STATE | LVIF_TEXT;
                var tc = ProcessMemoryChunk.Alloc(sw.Process, 301);
                lvi.pszText = tc.Location;
                var lc = ProcessMemoryChunk.AllocStruct(sw.Process, lvi);
                ApiHelper.FailIfZero(SystemWindow.SendMessage(new HandleRef(sw, sw.HWnd), LVM_GETITEM, IntPtr.Zero, lc.Location));
                lvi = (LVITEM) lc.ReadToStructure(0, typeof(LVITEM));
                lc.Dispose();
                if (lvi.pszText != tc.Location) {
                    tc.Dispose();
                    tc = new ProcessMemoryChunk(sw.Process, lvi.pszText, lvi.cchTextMax);
                }

                var tmp = tc.Read();
                var title = Encoding.Default.GetString(tmp);
                if (title.IndexOf('\0') != -1) title = title.Substring(0, title.IndexOf('\0'));
                var image = lvi.iImage;
                var state = lvi.state;
                tc.Dispose();
                return new SystemListViewItem(sw, index, title, state, image);
            }
        }

        /// <summary>
        ///     All columns of this list view, if it is in report view.
        /// </summary>
        public SystemListViewColumn[] Columns {
            get {
                var result = new List<SystemListViewColumn>();
                var lvc = new LVCOLUMN();
                lvc.cchTextMax = 300;
                lvc.mask = LVCF_FMT | LVCF_SUBITEM | LVCF_TEXT | LVCF_WIDTH;
                var tc = ProcessMemoryChunk.Alloc(sw.Process, 301);
                lvc.pszText = tc.Location;
                var lc = ProcessMemoryChunk.AllocStruct(sw.Process, lvc);
                for (var i = 0;; i++) {
                    var ok = SystemWindow.SendMessage(new HandleRef(sw, sw.HWnd), LVM_GETCOLUMN, new IntPtr(i), lc.Location);
                    if (ok == IntPtr.Zero) break;
                    lvc = (LVCOLUMN) lc.ReadToStructure(0, typeof(LVCOLUMN));
                    var tmp = tc.Read();
                    var title = Encoding.Default.GetString(tmp);
                    if (title.IndexOf('\0') != -1) title = title.Substring(0, title.IndexOf('\0'));
                    result.Add(new SystemListViewColumn(lvc.fmt, lvc.cx, lvc.iSubItem, title));
                }

                tc.Dispose();
                lc.Dispose();
                return result.ToArray();
            }
        }

        /// <summary>
        ///     Get a SystemListView reference from a SystemWindow (which is a list view)
        /// </summary>
        public static SystemListView FromSystemWindow(SystemWindow sw)
        {
            if (sw.SendGetMessage(LVM_GETITEMCOUNT) == 0) return null;
            return new SystemListView(sw);
        }

        #region PInvoke Declarations

        internal static readonly uint LVM_GETITEMRECT = (0x1000 + 14),
            LVM_SETITEMPOSITION = (0x1000 + 15),
            LVM_GETITEMPOSITION = (0x1000 + 16),
            LVM_GETITEMCOUNT = (0x1000 + 4),
            LVM_GETITEM = 0x1005,
            LVM_GETCOLUMN = (0x1000 + 25);

        private static readonly uint LVIF_TEXT = 0x1,
            LVIF_IMAGE = 0x2,
            LVIF_STATE = 0x8,
            LVCF_FMT = 0x1,
            LVCF_WIDTH = 0x2,
            LVCF_TEXT = 0x4,
            LVCF_SUBITEM = 0x8;

        [StructLayout(LayoutKind.Sequential)]
        private struct LVCOLUMN {
            public uint mask;
            public readonly int fmt;
            public readonly int cx;
            public IntPtr pszText;
            public int cchTextMax;
            public readonly int iSubItem;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LVITEM {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public readonly uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public readonly int iImage;
            public readonly IntPtr lParam;
        }

        #endregion
    }

    /// <summary>
    ///     An item of a list view.
    /// </summary>
    public class SystemListViewItem {
        private readonly int index;
        private readonly SystemWindow sw;

        internal SystemListViewItem(SystemWindow sw, int index, string title, uint state, int image)
        {
            this.sw = sw;
            this.index = index;
            this.Title = title;
            this.State = state;
            this.Image = image;
        }

        /// <summary>
        ///     The title of this item
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     The index of this item's image in the image list of this list view.
        /// </summary>
        public int Image { get; }

        /// <summary>
        ///     State bits of this item.
        /// </summary>
        public uint State { get; }

        /// <summary>
        ///     Position of the upper left corner of this item.
        /// </summary>
        public Point Position {
            get {
                var pt = new POINT();
                var c = ProcessMemoryChunk.AllocStruct(sw.Process, pt);
                ApiHelper.FailIfZero(SystemWindow.SendMessage(new HandleRef(sw, sw.HWnd), SystemListView.LVM_GETITEMPOSITION, new IntPtr(index), c.Location));
                pt = (POINT) c.ReadToStructure(0, typeof(POINT));
                return new Point(pt.X, pt.Y);
            }
            set => SystemWindow.SendMessage(new HandleRef(sw, sw.HWnd), SystemListView.LVM_SETITEMPOSITION, new IntPtr(index), new IntPtr(value.X + (value.Y << 16)));
        }

        /// <summary>
        ///     Bounding rectangle of this item.
        /// </summary>
        public RECT Rectangle {
            get {
                var r = new RECT();
                var c = ProcessMemoryChunk.AllocStruct(sw.Process, r);
                SystemWindow.SendMessage(new HandleRef(sw, sw.HWnd), SystemListView.LVM_GETITEMRECT, new IntPtr(index), c.Location);
                r = (RECT) c.ReadToStructure(0, typeof(RECT));
                return r;
            }
        }
    }

    /// <summary>
    ///     A column of a list view.
    /// </summary>
    public class SystemListViewColumn {
        internal SystemListViewColumn(int format, int width, int subIndex, string title)
        {
            this.Format = format;
            this.Width = width;
            this.SubIndex = subIndex;
            this.Title = title;
        }

        /// <summary>
        ///     The format (like left justified) of this column.
        /// </summary>
        public int Format { get; }

        /// <summary>
        ///     The width of this column.
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     The subindex of the subitem displayed in this column. Note
        ///     that the second column does not necessarily display the second
        ///     subitem - especially when the columns can be reordered by the user.
        /// </summary>
        public int SubIndex { get; }

        /// <summary>
        ///     The title of this column.
        /// </summary>
        public string Title { get; }
    }
}
