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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;


#nullable enable

namespace Switcheroo {
    public class FormattedTextAttribute {
        /// <summary>
        /// Dynamically set XAML formatted text
        /// </summary>
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            name: "FormattedText",
            propertyType: typeof(List<Inline>),
            ownerType: typeof(FormattedTextAttribute),
            defaultMetadata: new UIPropertyMetadata(new List<Inline>(), FormattedTextChanged)
        );

        public static void SetFormattedText(DependencyObject textBlock, List<Inline>? value)
        {
            textBlock.SetValue(FormattedTextProperty, value);
        }

        public static List<Inline>? GetFormattedText(DependencyObject textBlock)
        {
            return (List<Inline>?) textBlock.GetValue(FormattedTextProperty);
        }

        private static void FormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock) {
                return;
            }

            var inlines = (List<Inline>) e.NewValue ?? new List<Inline>();
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(inlines);
        }
    }
}
