using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;


namespace ManagedWinapi {
    /// <summary>
    ///     The unicode range of codepoints supported by a font.
    /// </summary>
    public class CodepointRange {
        private readonly char[] ranges;

        /// <summary>
        ///     Creates a new CodepointRange object for a font.
        /// </summary>
        public CodepointRange(Font font)
        {
            var rangeList = new List<char>();
            var g = Graphics.FromImage(new Bitmap(1, 1));
            var hdc = g.GetHdc();
            var hFont = font.ToHfont();
            var oldFont = SelectObject(hdc, hFont);
            var size = GetFontUnicodeRanges(hdc, IntPtr.Zero);
            var glyphSet = Marshal.AllocHGlobal((int) size);
            GetFontUnicodeRanges(hdc, glyphSet);
            SupportedCodepointCount = Marshal.ReadInt32(glyphSet, 8);
            var tmp = 0;
            var count = Marshal.ReadInt32(glyphSet, 12);
            for (var i = 0; i < count; i++) {
                var firstIncluded = (char) Marshal.ReadInt16(glyphSet, 16 + i * 4);
                var firstExcluded = (char) (firstIncluded + Marshal.ReadInt16(glyphSet, 18 + i * 4));
                tmp += firstExcluded - firstIncluded;
                rangeList.Add(firstIncluded);
                rangeList.Add(firstExcluded);
            }

            SelectObject(hdc, oldFont);
            DeleteObject(hFont);
            Marshal.FreeHGlobal(glyphSet);
            g.ReleaseHdc(hdc);
            g.Dispose();
            if (tmp != SupportedCodepointCount) throw new Exception(font.FontFamily.Name);
            ranges = rangeList.ToArray();
            if (ranges.Length < 2) throw new Exception();
        }

        /// <summary>
        ///     The number of codepoints supported by this font.
        /// </summary>
        public int SupportedCodepointCount { get; }

        /// <summary>
        ///     The first (lowest) supported codepoint.
        /// </summary>
        public char FirstCodepoint => ranges[0];

        /// <summary>
        ///     The last (highest) supported codepoint.
        /// </summary>
        public char LastCodepoint => (char) (ranges[ranges.Length - 1] - 1);

        /// <summary>
        ///     Returns a dictionary containing codepoint ranges of all fonts.
        ///     If multiple fonts of one family (bold, italic, etc.) share their
        ///     codepoint range, only their base font is included in this list,
        ///     otherwise all different variants are included.
        /// </summary>
        public static Dictionary<Font, CodepointRange> GetRangesForAllFonts()
        {
            var result = new Dictionary<Font, CodepointRange>();
            foreach (var ff in FontFamily.Families) {
                var fonts = new Font[16];
                var range = new CodepointRange[fonts.Length];
                for (var i = 0; i < fonts.Length; i++) {
                    if (ff.IsStyleAvailable((FontStyle) i)) {
                        fonts[i] = new Font(ff, 10, (FontStyle) i);
                        range[i] = new CodepointRange(fonts[i]);
                    }
                }

                var importantBits = 0;
                for (var bit = 1; bit < fonts.Length; bit <<= 1) {
                    for (var i = 0; i < fonts.Length; i++) {
                        if ((i & bit) != 0) continue;
                        if (range[i] != null && range[i | bit] != null) {
                            if (!range[i].Equals(range[i | bit])) {
                                importantBits |= bit;
                                break;
                            }
                        }
                        else if (range[i] != null || range[i | bit] != null) {
                            importantBits |= bit;
                            break;
                        }
                    }
                }

                for (var i = 0; i < fonts.Length; i++) {
                    if ((i & importantBits) != i || fonts[i] == null) continue;
                    result.Add(fonts[i], range[i]);
                }
            }

            return result;
        }

        /// <summary>
        ///     Tests whether a specific codepoint is supported by this font.
        /// </summary>
        public bool IsSupported(char codepoint)
        {
            var result = false;
            foreach (var c in ranges) {
                if (c > codepoint) break;
                result = !result;
            }

            return result;
        }

        /// <summary>
        ///     Finds the next codepoint that is either supported or not.
        /// </summary>
        public char FindNext(char from, bool supported)
        {
            if (IsSupported(from) == supported) return from;
            foreach (var c in ranges) {
                if (c > from) return c;
            }

            return (char) 0xFFFF;
        }

        /// <summary>
        ///     Returns a <see cref="string" /> representation of this codepoint range.
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < ranges.Length; i++) {
                if (i % 2 == 1) {
                    if (ranges[i] == ranges[i - 1] + 1) continue;
                    sb.Append("-");
                }
                else if (i != 0) {
                    sb.Append(", ");
                }

                sb.Append((ranges[i] - i % 2).ToString("X4"));
            }

            return sb.Append("]").ToString();
        }

        #region Equals and HashCode

        ///
        public override bool Equals(object obj)
        {
            var cr = obj as CodepointRange;
            if (cr == null)
                return false;
            if (SupportedCodepointCount != cr.SupportedCodepointCount || ranges.Length != cr.ranges.Length)
                return false;
            for (var i = 0; i < ranges.Length; i++) {
                if (ranges[i] != cr.ranges[i]) {
                    return false;
                }
            }

            return true;
        }

        ///
        public override int GetHashCode()
        {
            return 3 * SupportedCodepointCount + 7 * ranges.Length + 9 * FirstCodepoint + 11 * LastCodepoint;
        }

        #endregion

        #region PInvoke Declarations

        [DllImport("gdi32.dll")]
        private static extern uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion
    }
}
