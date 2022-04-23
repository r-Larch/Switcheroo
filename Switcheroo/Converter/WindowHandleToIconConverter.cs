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
using System.Globalization;
using System.Runtime.Caching;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Switcheroo.Windows;


namespace Switcheroo {
    public class WindowHandleToIconConverter : IValueConverter {
        private readonly IconToBitmapImageConverter _iconToBitmapConverter;

        public WindowHandleToIconConverter()
        {
            _iconToBitmapConverter = new IconToBitmapImageConverter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var handle = (IntPtr) value;
            var key = $"IconImage-{handle}";
            var shortCacheKey = $"{key}-shortCache";
            var longCacheKey = $"{key}-longCache";

            if (MemoryCache.Default.Get(shortCacheKey) is not BitmapImage iconImage) {
                var window = new AppWindow(handle);
                var icon = ShouldUseSmallTaskbarIcons() ? window.SmallWindowIcon : window.LargeWindowIcon;
                iconImage = _iconToBitmapConverter.Convert(icon) ?? new BitmapImage();
                MemoryCache.Default.Set(shortCacheKey, iconImage, DateTimeOffset.Now.AddSeconds(5));
                MemoryCache.Default.Set(longCacheKey, iconImage, DateTimeOffset.Now.AddMinutes(120));
            }

            return iconImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static bool ShouldUseSmallTaskbarIcons()
        {
            const string cacheKey = "SmallTaskbarIcons";

            if (MemoryCache.Default.Get(cacheKey) is bool cachedSetting) {
                return cachedSetting;
            }

            using (var registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced")) {
                var value = registryKey?.GetValue("TaskbarSmallIcons");
                if (value == null) {
                    return false;
                }

                _ = int.TryParse(value.ToString(), out var intValue);
                var smallTaskbarIcons = intValue == 1;
                MemoryCache.Default.Set(cacheKey, smallTaskbarIcons, DateTimeOffset.Now.AddMinutes(120));
                return smallTaskbarIcons;
            }
        }
    }
}
